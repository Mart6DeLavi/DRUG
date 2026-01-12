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
    public float heightOffset = 0.5f;    // Vertical offset above tile TOP
}

[System.Serializable]
public class LevelConfig
{
    [Tooltip("Additional Y offset for this level (based on: transform.y + levelSpacing * index).")]
    public float extraYOffset = 0f;

    [Tooltip("Gap multiplier between platforms (>1 = rarer, <1 = denser).")]
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
    /// Generation state for one level (lane).
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
    [SerializeField] private float tileHeight = 1f;   // Height of tile in world units (important for spawns and lava)

    [Header("Platform Length (core tiles)")]
    [SerializeField] private int minTiles = 4;
    [SerializeField] private int maxTiles = 10;

    [Header("Start Platform")]
    [Tooltip("Number of core grass tiles on the first (safe) platform (only on start level).")]
    [SerializeField] private int startPlatformGrassTiles = 5;

    [Header("Horizontal Gaps (world units)")]
    [SerializeField] private float minGap = 1.5f;
    [SerializeField] private float maxGap = 3.5f;

    [Header("Vertical Constraints (for jumps within one level)")]
    [SerializeField] private float maxStepUp = 1.5f;  // Max allowed up step (added to lastY)
    [SerializeField] private float maxStepDown = 3f;  // Max allowed down step
    [SerializeField] private float minY = -3f;
    [SerializeField] private float maxY = 5f;

    [Header("Generation Range")]
    [SerializeField] private float aheadDistance = 25f;
    [SerializeField] private float behindDistance = 20f;

    // -------------------------------------
    // LAVA
    // -------------------------------------
    [Header("Lava Settings")]
    [Tooltip("Chance that a platform will contain a lava section.")]
    [SerializeField] private float lavaChancePerPlatform = 0.4f;
    [SerializeField] private int minLavaRun = 1;   // will be forced to min 2 in code
    [SerializeField] private int maxLavaRun = 3;

    [Header("Lava Kill Zone")]
    [Tooltip("Height of kill-zone part above lava surface.")]
    [SerializeField] private float lavaKillZoneHeightAbove = 0.25f;

    [Tooltip("How deep kill-zone goes into tile (downward).")]
    [SerializeField] private float lavaKillZoneDepthBelow = 0.25f;

    [Tooltip("Tag assigned to lava tiles (death trigger).")]
    [SerializeField] private string lavaTag = "Obstacle";

    // -------------------------------------
    // SPAWN RULES (TRAPS / BONUS / DEBUFF)
    // -------------------------------------
    [Header("Trap Rules")]
    [SerializeField] private List<SpawnableObjectRule> trapRules = new List<SpawnableObjectRule>();

    [Header("Bonus Rules")]
    [SerializeField] private List<SpawnableObjectRule> bonusRules = new List<SpawnableObjectRule>();

    [Header("Debuff Rules")]
    [SerializeField] private List<SpawnableObjectRule> debuffRules = new List<SpawnableObjectRule>();


    // -------------------------------------
    // MOVING PLATFORMS
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

    [Tooltip("Optional per-level configuration (size <= levels). Missing entry = default gapMultiplier=1, extraYOffset=0.")]
    [SerializeField] private List<LevelConfig> levelConfigs = new List<LevelConfig>();

    // -------------------------------------
    // AVOIDING OVERLAP
    // -------------------------------------
    [Header("Platform Overlap Avoidance")]
    [Tooltip("If true, generator will try to move platforms vertically so they're not too close to others.")]
    [SerializeField] private bool avoidPlatformOverlap = true;

    [Tooltip("Minimal vertical distance between segment centers if they overlap in X axis.")]
    [SerializeField] private float minPlatformVerticalDistance = 1.5f;

    [Tooltip("Maximum number of attempts to shift platform up/down to find free space.")]
    [SerializeField] private int maxRelocateAttempts = 6;

    // -------------------------------------
    // DIFFICULTY SCALING
    // -------------------------------------
    [Header("Difficulty Scaling")]
    [Tooltip("If true, some spawn chances (lava, traps) grow over time.")]
    [SerializeField] private bool enableDifficultyScaling = true;

    [Tooltip("How much per second to ADD to lavaChancePerPlatform (0.05 = +5% chance for lava per second).")]
    [SerializeField] private float lavaChanceIncreasePerSecond = 0f;

    [Tooltip("How much per second to ADD to trap spawnChance in all trap rules.")]
    [SerializeField] private float trapSpawnChanceIncreasePerSecond = 0f;

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
    private float worldStartX;    // X position where generator started
    private float worldStartTime; // Time.time at start

    // Reusable buffer for physics shape points
    private readonly List<Vector2> shapeBuffer = new List<Vector2>(32);

    // -------------------------------------
    // UNITY LIFECYCLE
    // -------------------------------------
    private void Awake()
    {
        if (useFixedSeed)
        {
            Random.InitState(seed);
        }

        worldStartX = transform.position.x;
        worldStartTime = Time.time;
    }

    private void Start()
    {
        if (player == null || tilePrefab == null)
        {
            Debug.LogError("PlatformGenerator: Missing player or tilePrefab reference.");
            enabled = false;
            return;
        }

        InitLanes();
        GenerateInitialPlatforms();
    }

    private void Update()
    {
        if (player == null) return;

        float targetX = player.position.x + aheadDistance;
        EnsureGeneratedUpTo(targetX);
        CleanupBehind(player.position.x - behindDistance);
    }

    // -------------------------------------
    // LANES INIT
    // -------------------------------------
    private void InitLanes()
    {
        levels = Mathf.Max(1, levels);
        lanes = new LaneState[levels];

        for (int i = 0; i < levels; i++)
        {
            LevelConfig cfg = GetLevelConfig(i);

            float baseY = transform.position.y + i * levelSpacing + cfg.extraYOffset;

            var lane = new LaneState
            {
                laneIndex = i,
                baseY = baseY,
                lastY = baseY,
                lastEndX = transform.position.x
            };

            lanes[i] = lane;
        }
    }

    private LevelConfig GetLevelConfig(int laneIndex)
    {
        if (levelConfigs == null || levelConfigs.Count == 0)
            return new LevelConfig(); // default

        if (laneIndex < 0 || laneIndex >= levelConfigs.Count)
            return new LevelConfig(); // default

        return levelConfigs[laneIndex];
    }

    // -------------------------------------
    // GENERATION LOOP
    // -------------------------------------
    private void GenerateInitialPlatforms()
    {
        float targetX = player.position.x + aheadDistance;

        foreach (var lane in lanes)
        {
            while (lane.lastEndX < targetX)
            {
                SpawnNextPlatform(lane);
            }
        }
    }

    private void EnsureGeneratedUpTo(float targetX)
    {
        foreach (var lane in lanes)
        {
            while (lane.lastEndX < targetX)
            {
                SpawnNextPlatform(lane);
            }
        }
    }

    private void CleanupBehind(float minX)
    {
        foreach (var lane in lanes)
        {
            for (int i = lane.segments.Count - 1; i >= 0; i--)
            {
                PlatformSegmentMarker seg = lane.segments[i];
                if (seg == null)
                {
                    lane.segments.RemoveAt(i);
                    continue;
                }

                if (seg.endX < minX)
                {
                    Destroy(seg.gameObject);
                    lane.segments.RemoveAt(i);
                }
            }
        }
    }

    // -------------------------------------
    // SPAWN ONE PLATFORM SEGMENT
    // -------------------------------------
    private void SpawnNextPlatform(LaneState lane)
    {
        int laneIndex = lane.laneIndex;
        bool isStartLane = (laneIndex == startLevelIndex);
        bool isStartPlatform = lane.IsFirstPlatform && isStartLane;

        LevelConfig cfg = GetLevelConfig(laneIndex);
        float gapMult = Mathf.Max(0.1f, cfg.gapMultiplier);

        float gap = Random.Range(minGap, maxGap) * gapMult;
        float startX = lane.lastEndX + gap;

        // height
        float baseY = lane.lastY;
        float randomDeltaY = Random.Range(-maxStepDown, maxStepUp);
        float candidateY = Mathf.Clamp(baseY + randomDeltaY, minY, maxY);

        float segmentWidthEstimate = Random.Range(minTiles, maxTiles + 1) * tileWidth;
        float endXEstimate = startX + segmentWidthEstimate;
        float yFinal = FindNonOverlappingY(startX, endXEstimate, candidateY);

        lane.lastY = yFinal;

        // length
        int coreCount = isStartPlatform
            ? startPlatformGrassTiles
            : Random.Range(minTiles, maxTiles + 1);

        // GRASS / LAVA materials with difficulty scaling
        // Determine materials for this platform. On the very first start platform we enforce only grass tiles
        // so that the player always begins on a safe area without lava.
        TileMaterial[] materials;
        if (isStartPlatform)
        {
            materials = new TileMaterial[coreCount];
            for (int iMaterial = 0; iMaterial < coreCount; iMaterial++)
            {
                materials[iMaterial] = TileMaterial.Grass;
            }
        }
        else
        {
            materials = GenerateCoreMaterials(coreCount);
        }

        // Segment GameObject
        GameObject segmentGO = new GameObject($"PlatformSegment_L{laneIndex}");
        segmentGO.transform.position = new Vector3(startX, yFinal, 0f);

        var marker = segmentGO.AddComponent<PlatformSegmentMarker>();

        float segmentStartX = startX;
        float segmentEndX = startX + coreCount * tileWidth;
        marker.startX = segmentStartX;
        marker.endX = segmentEndX;
        marker.yCenter = yFinal;

        lane.segments.Add(marker);
        lane.lastEndX = segmentEndX;

        // collider happens on tiles – here only rigidbody
        Rigidbody2D rb = segmentGO.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        // MOVING PLATFORM (entire segment)
        // Do not add moving behaviour to the very first start platform
        if (!isStartPlatform)
        {
            TryAddMovingPlatform(segmentGO);
        }

        // counts per platform for spawn rules
        int[] trapCounts = trapRules.Count > 0 ? new int[trapRules.Count] : null;
        int[] bonusCounts = bonusRules.Count > 0 ? new int[bonusRules.Count] : null;
        int[] debuffCounts = debuffRules.Count > 0 ? new int[debuffRules.Count] : null;

        bool isStartPlat = isStartPlatform;
        float distanceFromStart = segmentStartX - worldStartX;

        // creating tiles
        for (int i = 0; i < coreCount; i++)
        {
            float worldX = startX + i * tileWidth;
            float worldY = yFinal; // tile center

            GameObject tile = Instantiate(tilePrefab, new Vector3(worldX, worldY, 0f), Quaternion.identity, segmentGO.transform);
            tile.name = $"Tile_{i}";

            TileMaterial mat = materials[i];
            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            if (sr == null) sr = tile.AddComponent<SpriteRenderer>();
            Sprite sprite = ChooseSpriteForTile(materials, i, coreCount);
            sr.sprite = sprite;

            // --- SOLID COLLIDER (for all tiles) ---
            bool isLava = (mat == TileMaterial.Lava);

            PolygonCollider2D solid = tile.AddComponent<PolygonCollider2D>();
            solid.isTrigger = false;   // always solid – also for lava

            if (sprite != null)
            {
                int shapeCount = sprite.GetPhysicsShapeCount();
                if (shapeCount == 0)
                {
                    // fallback: full rectangle
                    shapeBuffer.Clear();
                    Bounds b = sprite.bounds;
                    shapeBuffer.Add(new Vector2(b.min.x, b.min.y));
                    shapeBuffer.Add(new Vector2(b.max.x, b.min.y));
                    shapeBuffer.Add(new Vector2(b.max.x, b.max.y));
                    shapeBuffer.Add(new Vector2(b.min.x, b.max.y));

                    solid.pathCount = 1;
                    solid.SetPath(0, shapeBuffer.ToArray());
                }
                else
                {
                    solid.pathCount = shapeCount;
                    for (int s = 0; s < shapeCount; s++)
                    {
                        shapeBuffer.Clear();
                        sprite.GetPhysicsShape(s, shapeBuffer);
                        solid.SetPath(s, shapeBuffer.ToArray());
                    }
                }
            }

            // --- LAVA KILL TRIGGER ---
            if (isLava)
            {
                if (!string.IsNullOrEmpty(lavaTag))
                    tile.tag = lavaTag;

                AddLavaKillCollider(tile);
            }

            // DISAPPEARING TILES (only grass)
            if (enableDisappearingTiles &&
                mat == TileMaterial.Grass &&
                (!isStartPlat || disappearingOnStartPlatform))
            {
                if (Random.value <= disappearingChance)
                {
                    DisappearingTile dt = tile.AddComponent<DisappearingTile>();
                    dt.delayBeforeDisappear = disappearDelay;
                    dt.fadeDuration = fadeDuration;
                    dt.delayBeforeReappear = reappearDelay;
                    dt.affectWholeSegment = disappearWholeSegment;
                    dt.ignoreTriggerTiles = ignoreTriggerTilesOnDisappear;
                    // no groupRoot in your class – don't set it
                }
            }

            // SPAWN TRAP / BONUS / DEBUFF on top of tile
            float tileTopY = worldY + tileHeight * 0.5f;

            // Only spawn traps/bonuses/debuffs on platforms other than the initial start platform
            if (!isStartPlat)
            {
            TrySpawnAllRulesOnTile(
                mat,
                worldX,
                tileTopY,
                isStartPlat,
                distanceFromStart,
                segmentGO.transform,
                trapCounts,
                bonusCounts,
                debuffCounts,
                materials,
                i);
            }
        }
    }

    // -------------------------------------
    // LAVA KILL COLLIDER
    // -------------------------------------
    private void AddLavaKillCollider(GameObject tile)
    {
        // Box trigger covering lava top + part down, to also catch side jumps
        var kill = tile.AddComponent<BoxCollider2D>();
        kill.isTrigger = true;

        float above = Mathf.Max(0.01f, lavaKillZoneHeightAbove);
        float below = Mathf.Max(0f, lavaKillZoneDepthBelow);
        float totalHeight = above + below;

        kill.size = new Vector2(tileWidth, totalHeight);

        // assume pivot in tile center:
        // top edge of tile is at +tileHeight/2,
        // kill-zone should extend 'above' over top and 'below' down.
        float topLocalY = tileHeight * 0.5f;
        float centerLocalY = topLocalY - below + totalHeight * 0.5f;

        kill.offset = new Vector2(0f, centerLocalY);
    }

    // -------------------------------------
    // OVERLAP HELPERS
    // -------------------------------------
    private bool IsTooCloseToExisting(float startX, float endX, float yCandidate)
    {
        if (lanes == null) return false;
        if (minPlatformVerticalDistance <= 0f) return false;

        foreach (var lane in lanes)
        {
            foreach (var seg in lane.segments)
            {
                if (seg == null) continue;

                // no overlap in X axis => can ignore
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
            return Mathf.Clamp(initialY, minY, maxY);

        float baseY = Mathf.Clamp(initialY, minY, maxY);
        float candidate = baseY;

        for (int attempt = 0; attempt < maxRelocateAttempts; attempt++)
        {
            if (!IsTooCloseToExisting(startX, endX, candidate))
                return candidate;

            // Jump up / down further and further from base position
            int stepIndex = attempt / 2 + 1;
            float dir = (attempt % 2 == 0) ? 1f : -1f;
            float offset = dir * stepIndex * minPlatformVerticalDistance;

            candidate = Mathf.Clamp(baseY + offset, minY, maxY);
        }

        // If after so many attempts still too tight – oh well, we take the last variant.
        return candidate;
    }

    // -------------------------------------
    // SPAWN RULES
    // -------------------------------------
    private void TrySpawnAllRulesOnTile(
        TileMaterial tileMaterial,
        float worldX,
        float tileTopY,
        bool isStartPlatform,
        float distanceFromStart,
        Transform parent,
        int[] trapCounts,
        int[] bonusCounts,
        int[] debuffCounts,
        TileMaterial[] platformMaterials,
        int tileIndex)
    {
        GameObject trapInstance;
        if (TrySpawnFromRules(
                trapRules,
                trapCounts,
                tileMaterial,
                worldX,
                tileTopY,
                isStartPlatform,
                distanceFromStart,
                parent,
                trapSpawnChanceIncreasePerSecond,
                out trapInstance))
        {
            return;
        }

        if (TrySpawnFromRules(
                bonusRules,
                bonusCounts,
                tileMaterial,
                worldX,
                tileTopY,
                isStartPlatform,
                distanceFromStart,
                parent,
                0f,
                out _))
        {
            return;
        }

        TrySpawnFromRules(
            debuffRules,
            debuffCounts,
            tileMaterial,
            worldX,
            tileTopY,
            isStartPlatform,
            distanceFromStart,
            parent,
            0f,
            out _);
    }

    private bool TrySpawnFromRules(
        List<SpawnableObjectRule> rules,
        int[] counts,
        TileMaterial tileMaterial,
        float worldX,
        float tileTopY,
        bool isStartPlatform,
        float distanceFromStart,
        Transform parent,
        float extraChancePerSecond,
        out GameObject spawnedInstance)
    {
        spawnedInstance = null;

        if (rules == null || rules.Count == 0)
            return false;

        float elapsed = enableDifficultyScaling ? (Time.time - worldStartTime) : 0f;

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

            float effectiveChance = rule.spawnChance;
            if (extraChancePerSecond > 0f && enableDifficultyScaling && elapsed > 0f)
            {
                effectiveChance = Mathf.Clamp01(rule.spawnChance + extraChancePerSecond * elapsed);
            }

            if (Random.value > effectiveChance)
                continue;

            Vector2 spawnPos = new Vector2(worldX, tileTopY + rule.heightOffset);
            spawnedInstance = Instantiate(rule.prefab, spawnPos, Quaternion.identity, parent);

            if (counts != null && i < counts.Length)
                counts[i]++;

            return true;
        }

        return false;
    }


    // -------------------------------------
    // CORE MATERIALS (GRASS / LAVA) + DIFFICULTY
    // -------------------------------------
    private TileMaterial[] GenerateCoreMaterials(int coreCount)
    {
        var materials = new TileMaterial[coreCount];

        for (int i = 0; i < coreCount; i++)
            materials[i] = TileMaterial.Grass;

        int minRun = Mathf.Max(minLavaRun, 2); // force min 2

        float elapsed = enableDifficultyScaling ? (Time.time - worldStartTime) : 0f;
        float effectiveLavaChance = lavaChancePerPlatform;
        if (lavaChanceIncreasePerSecond > 0f && enableDifficultyScaling && elapsed > 0f)
        {
            effectiveLavaChance = Mathf.Clamp01(lavaChancePerPlatform + lavaChanceIncreasePerSecond * elapsed);
        }

        if (coreCount < minRun + 1 || Random.value > effectiveLavaChance)
            return materials;

        int maxRunClamped = Mathf.Clamp(maxLavaRun, minRun, coreCount - 1); // leave at least 1 grass
        if (maxRunClamped < minRun)
            return materials;

        int lavaRunLength = Random.Range(minRun, maxRunClamped + 1);

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

        // PLATFORM BOUNDARIES – zawsze grass_left / grass_right / lava_left / lava_right
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
    // MOVING PLATFORM SETUP
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