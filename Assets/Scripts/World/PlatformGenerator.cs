using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnableObjectRule
{
    [Header("Prefab & basic settings")]
    public string name;                  // Optional label for convenience
    public GameObject prefab;            // Prefab to spawn
    [Range(0f, 1f)]
    public float spawnChance = 0.1f;     // Probability per tile (0..1)

    [Header("Where can it spawn?")]
    public bool allowOnGrass = true;     // Allow spawning on grass tiles
    public bool allowOnLava = false;     // Allow spawning on lava tiles

    [Header("When can it spawn?")]
    public bool allowOnStartPlatform = false; // Can appear on the very first platform (only on start level)
    public float minDistanceFromStart = 0f;   // Min world X distance from generator start

    [Header("Per-platform limit")]
    public int maxPerPlatform = 1;       // 0 = unlimited on one platform

    [Header("Visual offset")]
    public float heightOffset = 0.5f;    // Vertical offset above tile
}

[System.Serializable]
public class LevelConfig
{
    [Tooltip("Dodatkowy offset Y dla tego poziomu (na bazie: transform.y + levelSpacing * index).")]
    public float extraYOffset = 0f;

    [Tooltip("Mnożnik przerw między platformami (>1 = rzadziej, <1 = gęściej).")]
    public float gapMultiplier = 1f;
}

public class PlatformGenerator : MonoBehaviour
{
    // -------------------------------------
    // INTERNAL TYPES
    // -------------------------------------
    private enum TileMaterial
    {
        Grass,
        Lava
    }

    /// <summary>
    /// Stan generacji dla jednego poziomu (linii).
    /// </summary>
    private class LaneState
    {
        public int laneIndex;
        public float lastEndX;
        public float lastY;
        public float baseY;
        public List<PlatformSegmentMarker> segments = new List<PlatformSegmentMarker>();

        public bool IsFirstPlatform => segments.Count == 0;
    }

    // -------------------------------------
    // REFERENCES
    // -------------------------------------
    [Header("References")]
    [SerializeField] private Transform player;        // Player to follow
    [SerializeField] private GameObject tilePrefab;   // Prefab with SpriteRenderer

    [Header("Grass Sprites")]
    [SerializeField] private Sprite grassLeftSprite;
    [SerializeField] private Sprite grassMiddleSprite;
    [SerializeField] private Sprite grassRightSprite;

    [Header("Lava Sprites")]
    [SerializeField] private Sprite lavaLeftSprite;
    [SerializeField] private Sprite lavaMiddleSprite;
    [SerializeField] private Sprite lavaRightSprite;

    [Header("Transition Sprites (Grass <-> Lava)")]
    [SerializeField] private Sprite lavaGrassLeftSprite;   // Grass tile on RIGHT in L->G
    [SerializeField] private Sprite lavaGrassRightSprite;  // Grass tile on LEFT in G->L
    [SerializeField] private Sprite lavaGroundLeftSprite;  // Lava tile on RIGHT in G->L
    [SerializeField] private Sprite lavaGroundRightSprite; // Lava tile on LEFT in L->G

    // -------------------------------------
    // PLATFORM SHAPE SETTINGS
    // -------------------------------------
    [Header("Tile Settings")]
    [SerializeField] private float tileWidth = 1f;    // Width of one tile in world units

    [Header("Platform Length (core tiles)")]
    [SerializeField] private int minTiles = 4;
    [SerializeField] private int maxTiles = 10;

    [Header("Start Platform")]
    [Tooltip("Number of core grass tiles on the first (safe) platform (only on start level).")]
    [SerializeField] private int startPlatformGrassTiles = 5;

    [Header("Horizontal Gaps (world units)")]
    [SerializeField] private float minGap = 1.5f;
    [SerializeField] private float maxGap = 3.5f;

    [Header("Vertical Constraints (dla skoków w ramach jednego levelu)")]
    [SerializeField] private float maxStepUp = 1.5f;  // Max allowed up step (dodawane do lastY)
    [SerializeField] private float maxStepDown = 3f;  // Max allowed down step
    [SerializeField] private float minY = -3f;
    [SerializeField] private float maxY = 5f;

    [Header("Generation Range")]
    [SerializeField] private float aheadDistance = 25f;
    [SerializeField] private float behindDistance = 20f;

    [Header("Lava Settings")]
    [Tooltip("Chance that a platform will contain a lava section.")]
    [SerializeField] private float lavaChancePerPlatform = 0.4f;
    [SerializeField] private int minLavaRun = 1;   // will be forced to min 2 in code
    [SerializeField] private int maxLavaRun = 3;

    [Header("Physics / Layers & Tags")]
    [Tooltip("Layer used for all tile colliders (should be in PlayerMovement.groundLayer).")]
    [SerializeField] private string groundLayerName = "Ground";
    [Tooltip("Tag for lava tiles (MUST match PlayerDeath Obstacle).")]
    [SerializeField] private string lavaTag = "Obstacle";

    // -------------------------------------
    // SPAWN RULES
    // -------------------------------------
    [Header("Spawn rules - TRAPS")]
    public List<SpawnableObjectRule> trapRules = new List<SpawnableObjectRule>();

    [Header("Spawn rules - BONUSES")]
    public List<SpawnableObjectRule> bonusRules = new List<SpawnableObjectRule>();

    [Header("Spawn rules - DEBUFFS")]
    public List<SpawnableObjectRule> debuffRules = new List<SpawnableObjectRule>();

    [Header("Dynamic Obstacle Settings")]
    [Tooltip("If true, every spawned obstacle on grass becomes dynamic.")]
    [SerializeField] private bool enableDynamicObstacleMovement = true;
    [Tooltip("Fraction of the safe grass span that dynamic obstacles are allowed to use for movement.")]
    [SerializeField, Range(0.1f, 1f)] private float dynamicTravelFraction = 0.5f;
    [Tooltip("Absolute cap for horizontal travel regardless of grass span.")]
    [SerializeField] private float maxDynamicHorizontalTravel = 2f;
    [Tooltip("Minimal useful travel distance. If the safe span is smaller the obstacle stays static.")]
    [SerializeField] private float minDynamicHorizontalTravel = 0.25f;
    [Tooltip("Padding kept from the lava boundary when computing available travel.")]
    [SerializeField] private float dynamicObstacleEdgePadding = 0.1f;

    // -------------------------------------
    // MOVING PLATFORMS (NEW)
    // -------------------------------------
    [Header("Moving Platforms")]
    [Tooltip("If true, randomly selected platform segments will move left-right.")]
    public bool enableMovingPlatforms = true;

    [Range(0f, 1f)]
    [Tooltip("Chance that a generated platform segment becomes a moving platform.")]
    public float movingPlatformChance = 0.15f;

    [Tooltip("Horizontal movement amplitude range (world units).")]
    public float minMoveAmplitude = 1f;
    public float maxMoveAmplitude = 3f;

    [Tooltip("Horizontal movement speed range.")]
    public float minMoveSpeed = 0.5f;
    public float maxMoveSpeed = 1.8f;

    // -------------------------------------
    // DISAPPEARING TILES
    // -------------------------------------
    [Header("Disappearing Tiles")]
    [SerializeField] private bool enableDisappearingTiles = true;
    [Tooltip("Chance that a single GRASS tile will become disappearing.")]
    [SerializeField, Range(0f, 1f)] private float disappearingChance = 0.15f;
    [Tooltip("Delay (seconds) from stepping on tile to starting fade/disappear.")]
    [SerializeField] private float disappearDelay = 0.3f;
    [Tooltip("How long fade from visible to invisible lasts.")]
    [SerializeField] private float fadeDuration = 0.25f;
    [Tooltip("Delay before reappearing. Set <= 0 for one-time disappearing.")]
    [SerializeField] private float reappearDelay = 0f;
    [Tooltip("Allow disappearing tiles also on very first safe start platform.")]
    [SerializeField] private bool disappearingOnStartPlatform = false;
    [Tooltip("If true, disappearing affects whole platform segment.")]
    [SerializeField] private bool disappearWholeSegment = true;
    [Tooltip("If true, tiles with isTrigger (lava) are ignored when fading/disabling.")]
    [SerializeField] private bool ignoreTriggerTilesOnDisappear = true;

    // -------------------------------------
    // MULTI-LEVEL SETTINGS
    // -------------------------------------
    [Header("Multi-level")]
    [Tooltip("How many vertical lanes of platforms to generate (1 = old behavior).")]
    [SerializeField] private int levels = 1;

    [Tooltip("Vertical spacing between lanes in world units.")]
    [SerializeField] private float levelSpacing = 3f;

    [Tooltip("Index of lane that is treated as 'start level' (with safe start platform). 0 = lowest.")]
    [SerializeField] private int startLevelIndex = 0;

    [Tooltip("Opcjonalna konfiguracja per poziom (rozmiar <= levels). Brak wpisu = domyślnie gapMultiplier=1, extraYOffset=0.")]
    [SerializeField] private List<LevelConfig> levelConfigs = new List<LevelConfig>();

    // -------------------------------------
    // AVOIDING OVERLAP
    // -------------------------------------
    [Header("Platform Overlap Avoidance")]
    [Tooltip("Jeśli true, generator będzie próbował przesuwać platformy w pionie tak, by nie były zbyt blisko innych.")]
    [SerializeField] private bool avoidPlatformOverlap = true;

    [Tooltip("Minimalny pionowy dystans między środkami segmentów, jeśli zachodzą na siebie w osi X.")]
    [SerializeField] private float minPlatformVerticalDistance = 1.5f;

    [Tooltip("Maksymalna liczba prób przesunięcia platformy w górę/dół, żeby znaleźć wolne miejsce.")]
    [SerializeField] private int maxRelocateAttempts = 6;

    // -------------------------------------
    // DEBUG / SEED
    // -------------------------------------
    [Header("Debug / Seed")]
    [SerializeField] private bool useFixedSeed = false;
    [SerializeField] private int seed = 12345;

    // -------------------------------------
    // INTERNAL STATE
    // -------------------------------------
    private LaneState[] lanes;
    private int groundLayer;
    private float worldStartX; // X position where generator started

    // Reusable buffer for physics shape points
    private readonly List<Vector2> shapeBuffer = new List<Vector2>(32);

    // -------------------------------------
    // HELPERS
    // -------------------------------------
    private LevelConfig GetLevelConfig(int laneIndex)
    {
        if (levelConfigs == null || levelConfigs.Count == 0)
            return new LevelConfig(); // default

        if (laneIndex < 0 || laneIndex >= levelConfigs.Count)
            return new LevelConfig(); // default

        return levelConfigs[laneIndex];
    }

    private bool IsTooCloseToExisting(float startX, float endX, float yCandidate)
    {
        if (lanes == null) return false;
        if (minPlatformVerticalDistance <= 0f) return false;

        foreach (var lane in lanes)
        {
            foreach (var seg in lane.segments)
            {
                if (seg == null) continue;

                // brak overlapu w osi X => można olać
                if (seg.endX <= startX || seg.startX >= endX)
                    continue;

                float dy = Mathf.Abs(seg.yCenter - yCandidate);
                if (dy < minPlatformVerticalDistance)
                    return true;
            }
        }

        return false;
    }

    private float FindNonOverlappingY(float startX, float endX, float initialY)
    {
        if (!avoidPlatformOverlap)
            return initialY;

        float baseY = Mathf.Clamp(initialY, minY, maxY);
        float candidate = baseY;

        for (int attempt = 0; attempt < maxRelocateAttempts; attempt++)
        {
            if (!IsTooCloseToExisting(startX, endX, candidate))
                return candidate;

            // Skaczemy do góry / w dół coraz dalej od pozycji bazowej
            int stepIndex = attempt / 2 + 1;
            float dir = (attempt % 2 == 0) ? 1f : -1f;
            float offset = dir * stepIndex * minPlatformVerticalDistance;

            candidate = Mathf.Clamp(baseY + offset, minY, maxY);
        }

        // Jeśli po tylu próbach nadal ciasno – trudno, bierzemy ostatni wariant.
        return candidate;
    }

    // -------------------------------------
    // UNITY LIFECYCLE
    // -------------------------------------
    private void Awake()
    {
        if (useFixedSeed)
            Random.InitState(seed);

        groundLayer = LayerMask.NameToLayer(groundLayerName);
        if (groundLayer < 0)
        {
            Debug.LogError($"PlatformGenerator: Layer '{groundLayerName}' does not exist. Create it and assign to platforms & ground.");
            groundLayer = 0; // fallback Default
        }
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("PlatformGenerator: Player is not assigned.");
            enabled = false;
            return;
        }

        if (tilePrefab == null)
        {
            Debug.LogError("PlatformGenerator: tilePrefab is not assigned.");
            enabled = false;
            return;
        }

        // Clamp multi-level settings
        if (levels < 1) levels = 1;
        if (startLevelIndex < 0) startLevelIndex = 0;
        if (startLevelIndex >= levels) startLevelIndex = levels - 1;

        // Global start X
        worldStartX = transform.position.x;

        // Initialize lane states
        lanes = new LaneState[levels];
        for (int i = 0; i < levels; i++)
        {
            LaneState lane = new LaneState();
            lane.laneIndex = i;

            LevelConfig cfg = GetLevelConfig(i);

            float baseY = transform.position.y + i * levelSpacing + cfg.extraYOffset;
            baseY = Mathf.Clamp(baseY, minY, maxY);

            lane.baseY = baseY;
            lane.lastY = baseY;
            lane.lastEndX = transform.position.x;

            lanes[i] = lane;
        }

        // Initial generation for all lanes
        for (int i = 0; i < lanes.Length; i++)
        {
            bool isStartLevel = (i == startLevelIndex);
            LaneState lane = lanes[i];

            while (lane.lastEndX < player.position.x + aheadDistance)
            {
                SpawnNextPlatform(lane, lane.laneIndex, isStartLevel);
            }
        }
    }

    private void Update()
    {
        if (player == null || lanes == null) return;

        float playerX = player.position.x;

        // Generate ahead for each lane
        for (int i = 0; i < lanes.Length; i++)
        {
            LaneState lane = lanes[i];
            bool isStartLevel = (i == startLevelIndex);

            while (lane.lastEndX < playerX + aheadDistance)
            {
                SpawnNextPlatform(lane, lane.laneIndex, isStartLevel);
            }
        }

        // Cleanup behind for each lane
        foreach (var lane in lanes)
        {
            for (int i = lane.segments.Count - 1; i >= 0; i--)
            {
                var seg = lane.segments[i];
                if (seg == null)
                {
                    lane.segments.RemoveAt(i);
                    continue;
                }

                if (seg.endX < playerX - behindDistance)
                {
                    Destroy(seg.gameObject);
                    lane.segments.RemoveAt(i);
                }
            }
        }
    }

    // -------------------------------------
    // PLATFORM GENERATION PER LANE
    // -------------------------------------
    private void SpawnNextPlatform(LaneState lane, int laneIndex, bool isStartLevel)
    {
        LevelConfig cfg = GetLevelConfig(laneIndex);

        // Czy to zupełnie pierwsza platforma na tym torze?
        bool isLaneFirstPlatform = lane.IsFirstPlatform;
        // Czy to jednocześnie „główny” startowy level?
        bool isStartPlatform = isLaneFirstPlatform && isStartLevel;

        // Gap
        float baseGap = Random.Range(minGap, maxGap);
        float gapMultiplier = (cfg.gapMultiplier <= 0f) ? 1f : cfg.gapMultiplier;
        float gap = isStartPlatform ? 0f : baseGap * gapMultiplier;
        float startX = lane.lastEndX + gap;

        // Pierwsza platforma na levelu ma bazowe Y, kolejne skaczą w górę/dół
        float newY;
        if (isStartPlatform)
        {
            newY = lane.baseY;
        }
        else
        {
            float offsetY = Random.Range(-maxStepDown, maxStepUp);
            newY = Mathf.Clamp(lane.lastY + offsetY, minY, maxY);
        }

        int coreCount;
        TileMaterial[] coreMaterials;

        if (isStartPlatform)
        {
            // First platform on start level: only grass, safe for player
            coreCount = Mathf.Max(1, startPlatformGrassTiles);
            coreMaterials = new TileMaterial[coreCount];
            for (int i = 0; i < coreCount; i++)
                coreMaterials[i] = TileMaterial.Grass;
        }
        else
        {
            // Random platform with possible lava
            coreCount = Random.Range(minTiles, maxTiles + 1);
            coreMaterials = GenerateCoreMaterials(coreCount);
        }

        // Add 2 boundary tiles: left + right
        int visualCount = coreCount + 2;
        TileMaterial[] visualMaterials = new TileMaterial[visualCount];

        // Left boundary (index 0) - same material as first core tile
        visualMaterials[0] = coreMaterials[0];

        // Middle (1..visualCount-2) corresponds to core[0..coreCount-1]
        for (int ci = 0; ci < coreCount; ci++)
        {
            visualMaterials[ci + 1] = coreMaterials[ci];
        }

        // Right boundary (index last) - same material as last core tile
        visualMaterials[visualCount - 1] = coreMaterials[coreCount - 1];

        float totalWidth = visualCount * tileWidth;
        float endX = startX + totalWidth;

        // *** TU UNIKAMY NACHODZENIA SIĘ SEGMENTÓW ***
        float adjustedY = FindNonOverlappingY(startX, endX, newY);

        // Parent for the whole segment (no colliders here)
        GameObject segmentGO = new GameObject($"PlatformSegment_L{laneIndex}");
        segmentGO.transform.parent = transform;
        segmentGO.transform.position = Vector3.zero;
        segmentGO.layer = groundLayer;

        // Marker for cleanup i testów odległości
        PlatformSegmentMarker marker = segmentGO.AddComponent<PlatformSegmentMarker>();
        TryAddMovingPlatform(segmentGO);

        marker.startX = startX;
        marker.endX = endX;
        marker.yCenter = adjustedY;

        // Per-platform counters for spawn rules
        int[] trapCounts = (trapRules != null && trapRules.Count > 0) ? new int[trapRules.Count] : null;
        int[] bonusCounts = (bonusRules != null && bonusRules.Count > 0) ? new int[bonusRules.Count] : null;
        int[] debuffCounts = (debuffRules != null && debuffRules.Count > 0) ? new int[debuffRules.Count] : null;

        // Create tiles (each tile has its own collider)
        for (int i = 0; i < visualCount; i++)
        {
            float worldX = startX + i * tileWidth;
            float worldY = adjustedY;

            GameObject tile = Instantiate(tilePrefab, segmentGO.transform);
            tile.transform.position = new Vector2(worldX, worldY);
            tile.layer = groundLayer;

            // Remove any existing collider from tile instance
            var oldCol = tile.GetComponent<Collider2D>();
            if (oldCol != null)
                Destroy(oldCol);

            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = tile.AddComponent<SpriteRenderer>();

            // Sprite based on visual materials + neighbors
            sr.sprite = ChooseSpriteForTile(visualMaterials, i, visualCount);
            Sprite sprite = sr.sprite;
            if (sprite == null)
                continue;

            // Collider per tile, shape from sprite physics outline
            PolygonCollider2D poly = tile.AddComponent<PolygonCollider2D>();

            bool isLava = (visualMaterials[i] == TileMaterial.Lava);
            poly.isTrigger = isLava; // lava => trigger (death), grass => solid

            int shapeCount = sprite.GetPhysicsShapeCount();
            if (shapeCount == 0)
            {
                // Fallback: full rect from bounds
                shapeBuffer.Clear();
                Bounds b = sprite.bounds;
                shapeBuffer.Add(new Vector2(b.min.x, b.min.y));
                shapeBuffer.Add(new Vector2(b.max.x, b.min.y));
                shapeBuffer.Add(new Vector2(b.max.x, b.max.y));
                shapeBuffer.Add(new Vector2(b.min.x, b.max.y));

                poly.pathCount = 1;
                poly.SetPath(0, shapeBuffer.ToArray());
            }
            else
            {
                poly.pathCount = shapeCount;
                for (int s = 0; s < shapeCount; s++)
                {
                    shapeBuffer.Clear();
                    sprite.GetPhysicsShape(s, shapeBuffer);
                    poly.SetPath(s, shapeBuffer.ToArray());
                }
            }

            // Tags for lava
            if (isLava && !string.IsNullOrEmpty(lavaTag))
            {
                tile.tag = lavaTag; // PlayerDeath checks this
            }

            // === DISAPPEARING TILES (only grass) ===
            if (enableDisappearingTiles && !isLava)
            {
                bool allowHere = !isStartPlatform || disappearingOnStartPlatform;
                if (allowHere && Random.value < disappearingChance)
                {
                    DisappearingTile dt = tile.AddComponent<DisappearingTile>();
                    dt.delayBeforeDisappear = disappearDelay;
                    dt.fadeDuration = fadeDuration;
                    dt.delayBeforeReappear = reappearDelay;
                    dt.affectWholeSegment = disappearWholeSegment;
                    dt.ignoreTriggerTiles = ignoreTriggerTilesOnDisappear;
                }
            }

            // Try to spawn traps / bonuses / debuffs on this tile
            TrySpawnObjectsOnTile(
                visualMaterials[i],
                worldX,
                worldY,
                isStartPlatform,
                segmentGO.transform,
                visualMaterials,
                i,
                trapRules,
                trapCounts,
                bonusRules,
                bonusCounts,
                debuffRules,
                debuffCounts
            );
        }

        lane.lastEndX = endX;
        lane.lastY = adjustedY;
        lane.segments.Add(marker);
    }

    // -------------------------------------
    // SPAWN OBJECTS (TRAPS / BONUSES / DEBUFFS)
    // -------------------------------------
    private void TrySpawnObjectsOnTile(
        TileMaterial tileMaterial,
        float worldX,
        float worldY,
        bool isStartPlatform,
        Transform parent,
        TileMaterial[] platformMaterials,
        int tileIndex,
        List<SpawnableObjectRule> trapRules,
        int[] trapCounts,
        List<SpawnableObjectRule> bonusRules,
        int[] bonusCounts,
        List<SpawnableObjectRule> debuffRules,
        int[] debuffCounts)
    {
        float distanceFromStart = worldX - worldStartX;

        if (TrySpawnFromRules(
                trapRules,
                trapCounts,
                tileMaterial,
                worldX,
                worldY,
                isStartPlatform,
                distanceFromStart,
                parent,
                out GameObject trapInstance))
        {
            TryEnableDynamicObstacle(trapInstance, tileMaterial, platformMaterials, tileIndex);
            return;
        }

        if (TrySpawnFromRules(
                bonusRules,
                bonusCounts,
                tileMaterial,
                worldX,
                worldY,
                isStartPlatform,
                distanceFromStart,
                parent,
                out _))
        {
            return;
        }

        TrySpawnFromRules(
            debuffRules,
            debuffCounts,
            tileMaterial,
            worldX,
            worldY,
            isStartPlatform,
            distanceFromStart,
            parent,
            out _);
    }

    private bool TrySpawnFromRules(
        List<SpawnableObjectRule> rules,
        int[] counts,
        TileMaterial tileMaterial,
        float worldX,
        float worldY,
        bool isStartPlatform,
        float distanceFromStart,
        Transform parent,
        out GameObject spawnedInstance)
    {
        spawnedInstance = null;

        if (rules == null || rules.Count == 0)
            return false;

        for (int i = 0; i < rules.Count; i++)
        {
            SpawnableObjectRule rule = rules[i];
            if (rule == null || rule.prefab == null)
                continue;

            if (isStartPlatform && !rule.allowOnStartPlatform)
                continue;

            if (distanceFromStart < rule.minDistanceFromStart)
                continue;

            bool canOnGrass = tileMaterial == TileMaterial.Grass && rule.allowOnGrass;
            bool canOnLava = tileMaterial == TileMaterial.Lava && rule.allowOnLava;
            if (!canOnGrass && !canOnLava)
                continue;

            int alreadySpawned = (counts != null && i < counts.Length) ? counts[i] : 0;
            if (rule.maxPerPlatform > 0 && alreadySpawned >= rule.maxPerPlatform)
                continue;

            if (Random.value > rule.spawnChance)
                continue;

            Vector2 spawnPos = new Vector2(worldX, worldY + rule.heightOffset);
            spawnedInstance = Instantiate(rule.prefab, spawnPos, Quaternion.identity, parent);

            if (counts != null && i < counts.Length)
                counts[i]++;

            return true;
        }

        return false;
    }

    private void TryEnableDynamicObstacle(
        GameObject obstacleInstance,
        TileMaterial tileMaterial,
        TileMaterial[] platformMaterials,
        int tileIndex)
    {
        if (!enableDynamicObstacleMovement || obstacleInstance == null)
            return;

        if (tileMaterial != TileMaterial.Grass)
            return;

        float safeTravel = CalculateDynamicTravelWithinGrass(platformMaterials, tileIndex);
        if (safeTravel <= 0f)
            return;

        float cappedTravel = Mathf.Min(maxDynamicHorizontalTravel, safeTravel);
        cappedTravel *= Mathf.Clamp01(dynamicTravelFraction);
        cappedTravel = Mathf.Min(cappedTravel, safeTravel);

        if (cappedTravel < minDynamicHorizontalTravel)
        {
            if (safeTravel >= minDynamicHorizontalTravel)
                cappedTravel = minDynamicHorizontalTravel;
            else
                cappedTravel = safeTravel;
        }

        if (cappedTravel <= 0f)
            return;

        DynamicObstacle dynamicObstacle = obstacleInstance.GetComponent<DynamicObstacle>();
        if (dynamicObstacle == null)
            dynamicObstacle = obstacleInstance.AddComponent<DynamicObstacle>();

        dynamicObstacle.SetLocalTravel(new Vector2(cappedTravel, 0f));
    }

    private float CalculateDynamicTravelWithinGrass(TileMaterial[] platformMaterials, int tileIndex)
    {
        if (platformMaterials == null || tileIndex < 0 || tileIndex >= platformMaterials.Length)
            return 0f;

        if (platformMaterials[tileIndex] != TileMaterial.Grass)
            return 0f;

        int left = tileIndex;
        while (left > 0 && platformMaterials[left - 1] == TileMaterial.Grass)
            left--;

        int right = tileIndex;
        while (right < platformMaterials.Length - 1 && platformMaterials[right + 1] == TileMaterial.Grass)
            right++;

        float tilesToLeftEdge = (tileIndex - left + 0.5f) * tileWidth;
        float tilesToRightEdge = (right - tileIndex + 0.5f) * tileWidth;
        float padding = Mathf.Max(0f, dynamicObstacleEdgePadding);

        float leftDistance = Mathf.Max(0f, tilesToLeftEdge - padding);
        float rightDistance = Mathf.Max(0f, tilesToRightEdge - padding);

        float safeTravel = 2f * Mathf.Min(leftDistance, rightDistance);
        return Mathf.Max(0f, safeTravel);
    }

    // -------------------------------------
    // CORE MATERIALS (GRASS / LAVA)
    // -------------------------------------
    private TileMaterial[] GenerateCoreMaterials(int coreCount)
    {
        var materials = new TileMaterial[coreCount];

        for (int i = 0; i < coreCount; i++)
            materials[i] = TileMaterial.Grass;

        int minRun = Mathf.Max(minLavaRun, 2); // force min 2
        if (coreCount < minRun + 1 || Random.value > lavaChancePerPlatform)
            return materials;

        int maxRun = Mathf.Clamp(maxLavaRun, minRun, coreCount - 1); // leave at least 1 grass
        if (maxRun < minRun)
            return materials;

        int lavaRunLength = Random.Range(minRun, maxRun + 1);

        int startMax = coreCount - lavaRunLength; // ensure full run fits
        int lavaStart = Random.Range(0, startMax + 1);
        int lavaEnd = lavaStart + lavaRunLength; // exclusive

        for (int i = lavaStart; i < lavaEnd; i++)
        {
            materials[i] = TileMaterial.Lava;
        }

        return materials;
    }

    // -------------------------------------
    // SPRITE CHOICE
    // -------------------------------------
    private Sprite ChooseSpriteForTile(TileMaterial[] materials, int i, int tilesCount)
    {
        TileMaterial current = materials[i];
        TileMaterial left = (i > 0) ? materials[i - 1] : current;
        TileMaterial right = (i < tilesCount - 1) ? materials[i + 1] : current;

        bool hasLeft = i > 0;
        bool hasRight = i < tilesCount - 1;

        bool leftSame = hasLeft && left == current;
        bool rightSame = hasRight && right == current;

        bool leftOther = hasLeft && left != current;
        bool rightOther = hasRight && right != current;

        // PLATFORM BOUNDARIES – always one of: grass_left / grass_right / lava_left / lava_right
        if (i == 0)
        {
            if (current == TileMaterial.Grass)
                return grassLeftSprite != null ? grassLeftSprite : grassMiddleSprite;
            else
                return lavaLeftSprite != null ? lavaLeftSprite : lavaMiddleSprite;
        }

        if (i == tilesCount - 1)
        {
            if (current == TileMaterial.Grass)
                return grassRightSprite != null ? grassRightSprite : grassMiddleSprite;
            else
                return lavaRightSprite != null ? lavaRightSprite : lavaMiddleSprite;
        }

        // MIDDLE
        if (current == TileMaterial.Grass)
        {
            if (leftOther && left == TileMaterial.Lava)
            {
                // Pair: lava_ground_right | lava_grass_left
                return lavaGrassLeftSprite != null ? lavaGrassLeftSprite : grassMiddleSprite;
            }
            if (rightOther && right == TileMaterial.Lava)
            {
                // Pair: lava_grass_right | lava_ground_left
                return lavaGrassRightSprite != null ? lavaGrassRightSprite : grassMiddleSprite;
            }

            if (!leftSame && rightSame)
                return grassLeftSprite != null ? grassLeftSprite : grassMiddleSprite;
            if (leftSame && rightSame)
                return grassMiddleSprite;
            if (leftSame && !rightSame)
                return grassRightSprite != null ? grassRightSprite : grassMiddleSprite;

            return grassMiddleSprite;
        }
        else // Lava
        {
            if (leftOther && left == TileMaterial.Grass)
            {
                // Pair: lava_grass_right | lava_ground_left
                return lavaGroundLeftSprite != null ? lavaGroundLeftSprite : lavaMiddleSprite;
            }
            if (rightOther && right == TileMaterial.Grass)
            {
                // Pair: lava_ground_right | lava_grass_left
                return lavaGroundRightSprite != null ? lavaGroundRightSprite : lavaMiddleSprite;
            }

            if (!leftSame && rightSame)
                return lavaLeftSprite != null ? lavaLeftSprite : lavaMiddleSprite;
            if (leftSame && rightSame)
                return lavaMiddleSprite;
            if (leftSame && !rightSame)
                return lavaRightSprite != null ? lavaRightSprite : lavaMiddleSprite;

            return lavaMiddleSprite;
        }
    }

    // -------------------------------------
    // MOVING PLATFORM SETUP (NEW)
    // -------------------------------------
    private void TryAddMovingPlatform(GameObject segmentGO)
    {
        if (!enableMovingPlatforms)
            return;

        if (Random.value > movingPlatformChance)
            return;

        MovingPlatform mp = segmentGO.AddComponent<MovingPlatform>();

        mp.amplitude = Random.Range(minMoveAmplitude, maxMoveAmplitude);
        mp.speed = Random.Range(minMoveSpeed, maxMoveSpeed);
        mp.randomOffset = true;
    }

    // -------------------------------------
    // GIZMOS
    // -------------------------------------
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            new Vector3(player.position.x + aheadDistance, -100f, 0f),
            new Vector3(player.position.x + aheadDistance, 100f, 0f)
        );

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(player.position.x - behindDistance, -100f, 0f),
            new Vector3(player.position.x - behindDistance, 100f, 0f)
        );
    }
}
