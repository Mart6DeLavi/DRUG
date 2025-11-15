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
    public bool allowOnStartPlatform = false; // Can appear on the very first platform
    public float minDistanceFromStart = 0f;   // Min world X distance from generator start

    [Header("Per-platform limit")]
    public int maxPerPlatform = 1;       // 0 = unlimited on one platform

    [Header("Visual offset")]
    public float heightOffset = 0.5f;    // Vertical offset above tile
}

public class PlatformGenerator : MonoBehaviour
{
    private enum TileMaterial
    {
        Grass,
        Lava
    }

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

    [Header("Tile Settings")]
    [SerializeField] private float tileWidth = 1f;    // Width of one tile in world units

    [Header("Platform Length (core tiles)")]
    [SerializeField] private int minTiles = 4;
    [SerializeField] private int maxTiles = 10;

    [Header("Start Platform")]
    [Tooltip("Number of core grass tiles on the first (safe) platform.")]
    [SerializeField] private int startPlatformGrassTiles = 5;

    [Header("Horizontal Gaps (world units)")]
    [SerializeField] private float minGap = 1.5f;
    [SerializeField] private float maxGap = 3.5f;

    [Header("Vertical Constraints")]
    [SerializeField] private float maxStepUp = 1.5f;  // Max allowed up step
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

    [Header("Spawn rules - TRAPS")]
    public List<SpawnableObjectRule> trapRules = new List<SpawnableObjectRule>();

    [Header("Spawn rules - BONUSES")]
    public List<SpawnableObjectRule> bonusRules = new List<SpawnableObjectRule>();

    [Header("Spawn rules - DEBUFFS")]
    public List<SpawnableObjectRule> debuffRules = new List<SpawnableObjectRule>();

    [Header("Debug / Seed")]
    [SerializeField] private bool useFixedSeed = false;
    [SerializeField] private int seed = 12345;

    // Internal
    private readonly List<PlatformSegmentMarker> segments = new List<PlatformSegmentMarker>();
    private float lastEndX;
    private float lastY;
    private int groundLayer;
    private float worldStartX; // X position where generator started

    // Reusable buffer for physics shape points
    private readonly List<Vector2> shapeBuffer = new List<Vector2>(32);

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

        // Start position = this object position (place under player in the scene)
        lastEndX = transform.position.x;
        lastY = transform.position.y;
        worldStartX = transform.position.x;

        // Initial generation
        while (lastEndX < player.position.x + aheadDistance)
        {
            SpawnNextPlatform();
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Generate ahead
        while (lastEndX < player.position.x + aheadDistance)
        {
            SpawnNextPlatform();
        }

        // Cleanup behind
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            var seg = segments[i];
            if (seg == null)
            {
                segments.RemoveAt(i);
                continue;
            }

            if (seg.endX < player.position.x - behindDistance)
            {
                Destroy(seg.gameObject);
                segments.RemoveAt(i);
            }
        }
    }

    private void SpawnNextPlatform()
    {
        bool isFirstPlatform = (segments.Count == 0);

        // FIRST PLATFORM: no gap, no vertical offset, only grass
        float gap = isFirstPlatform ? 0f : Random.Range(minGap, maxGap);
        float startX = lastEndX + gap;

        float offsetY = isFirstPlatform ? 0f : Random.Range(-maxStepDown, maxStepUp);
        float newY = Mathf.Clamp(lastY + offsetY, minY, maxY);

        int coreCount;
        TileMaterial[] coreMaterials;

        if (isFirstPlatform)
        {
            // First platform: only grass
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

        // Parent for the whole segment (no colliders here)
        GameObject segmentGO = new GameObject("PlatformSegment");
        segmentGO.transform.parent = transform;
        segmentGO.transform.position = Vector3.zero;
        segmentGO.layer = groundLayer;

        // Marker for cleanup
        PlatformSegmentMarker marker = segmentGO.AddComponent<PlatformSegmentMarker>();
        marker.startX = startX;
        marker.endX = startX + totalWidth;

        // Per-platform counters for spawn rules
        int[] trapCounts = trapRules != null && trapRules.Count > 0 ? new int[trapRules.Count] : null;
        int[] bonusCounts = bonusRules != null && bonusRules.Count > 0 ? new int[bonusRules.Count] : null;
        int[] debuffCounts = debuffRules != null && debuffRules.Count > 0 ? new int[debuffRules.Count] : null;

        // Create tiles (each tile has its own collider)
        for (int i = 0; i < visualCount; i++)
        {
            float worldX = startX + i * tileWidth;
            float worldY = newY;

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
            poly.isTrigger = isLava;          // lava => trigger (death), grass => solid

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

            // Tags:
            if (isLava && !string.IsNullOrEmpty(lavaTag))
            {
                tile.tag = lavaTag; // PlayerDeath checks this
            }

            // Try to spawn traps / bonuses / debuffs on this tile
            TrySpawnObjectsOnTile(
                visualMaterials[i],
                worldX,
                worldY,
                isFirstPlatform,
                segmentGO.transform,
                trapRules,
                trapCounts,
                bonusRules,
                bonusCounts,
                debuffRules,
                debuffCounts
            );
        }

        lastEndX = marker.endX;
        lastY = newY;
        segments.Add(marker);
    }

    /// <summary>
    /// Tries to spawn traps, bonuses and debuffs on a single tile.
    /// IMPORTANT: at most ONE object can spawn on a tile.
    /// Priority: traps -> bonuses -> debuffs.
    /// </summary>
    private void TrySpawnObjectsOnTile(
        TileMaterial tileMaterial,
        float worldX,
        float worldY,
        bool isFirstPlatform,
        Transform parent,
        List<SpawnableObjectRule> trapRules,
        int[] trapCounts,
        List<SpawnableObjectRule> bonusRules,
        int[] bonusCounts,
        List<SpawnableObjectRule> debuffRules,
        int[] debuffCounts)
    {
        float distanceFromStart = worldX - worldStartX;
        bool tileOccupied = false; // once something is spawned, we stop

        // Local function returns true if it spawned something
        bool ProcessRules(List<SpawnableObjectRule> rules, int[] counts)
        {
            if (rules == null || rules.Count == 0 || counts == null)
                return false;

            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                if (rule.prefab == null)
                    continue;

                // Start platform allowed?
                if (isFirstPlatform && !rule.allowOnStartPlatform)
                    continue;

                // Distance constraint
                if (distanceFromStart < rule.minDistanceFromStart)
                    continue;

                // Surface compatibility
                bool canOnGrass = (tileMaterial == TileMaterial.Grass && rule.allowOnGrass);
                bool canOnLava = (tileMaterial == TileMaterial.Lava && rule.allowOnLava);
                if (!canOnGrass && !canOnLava)
                    continue;

                // Per-platform limit
                if (rule.maxPerPlatform > 0 && counts[i] >= rule.maxPerPlatform)
                    continue;

                // Chance roll
                if (Random.value > rule.spawnChance)
                    continue;

                // Spawn object
                Vector2 spawnPos = new Vector2(worldX, worldY + rule.heightOffset);
                Instantiate(rule.prefab, spawnPos, Quaternion.identity, parent);
                counts[i]++;

                return true; // we spawned something on this tile
            }

            return false;
        }

        // Priority order: traps -> bonuses -> debuffs
        if (!tileOccupied && ProcessRules(trapRules, trapCounts))
            tileOccupied = true;

        if (!tileOccupied && ProcessRules(bonusRules, bonusCounts))
            tileOccupied = true;

        if (!tileOccupied && ProcessRules(debuffRules, debuffCounts))
            tileOccupied = true;
    }

    /// <summary>
    /// Generates core tile materials for a single random platform, ensuring:
    /// - at least one Grass
    /// - every lava run has min length 2
    /// </summary>
    private TileMaterial[] GenerateCoreMaterials(int coreCount)
    {
        var materials = new TileMaterial[coreCount];

        // Everything starts as Grass
        for (int i = 0; i < coreCount; i++)
            materials[i] = TileMaterial.Grass;

        // If platform is too short or lava not chosen -> all grass
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

    /// <summary>
    /// Choose appropriate sprite based on visualMaterials (with boundaries at index 0 and last).
    /// </summary>
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
            // TRANSITION: Lava -> Grass (we are first Grass)
            if (leftOther && left == TileMaterial.Lava)
            {
                // Pair: lava_ground_right | lava_grass_left
                // We are Grass on the RIGHT => lava_grass_left
                return lavaGrassLeftSprite != null ? lavaGrassLeftSprite : grassMiddleSprite;
            }
            // TRANSITION: Grass -> Lava (we are last Grass)
            if (rightOther && right == TileMaterial.Lava)
            {
                // Pair: lava_grass_right | lava_ground_left
                // We are Grass on the LEFT => lava_grass_right
                return lavaGrassRightSprite != null ? lavaGrassRightSprite : grassMiddleSprite;
            }

            // Pure grass segment
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
            // TRANSITION: Grass -> Lava (we are first Lava)
            if (leftOther && left == TileMaterial.Grass)
            {
                // Pair: lava_grass_right | lava_ground_left
                // We are Lava on the RIGHT => lava_ground_left
                return lavaGroundLeftSprite != null ? lavaGroundLeftSprite : lavaMiddleSprite;
            }
            // TRANSITION: Lava -> Grass (we are last Lava)
            if (rightOther && right == TileMaterial.Grass)
            {
                // Pair: lava_ground_right | lava_grass_left
                // We are Lava on the LEFT => lava_ground_right
                return lavaGroundRightSprite != null ? lavaGroundRightSprite : lavaMiddleSprite;
            }

            // Pure lava segment
            if (!leftSame && rightSame)
                return lavaLeftSprite != null ? lavaLeftSprite : lavaMiddleSprite;
            if (leftSame && rightSame)
                return lavaMiddleSprite;
            if (leftSame && !rightSame)
                return lavaRightSprite != null ? lavaRightSprite : lavaMiddleSprite;

            return lavaMiddleSprite;
        }
    }

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
