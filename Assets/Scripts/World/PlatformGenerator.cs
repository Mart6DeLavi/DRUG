using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private int minLavaRun = 1;   // będzie wymuszone min 2 w kodzie
    [SerializeField] private int maxLavaRun = 3;

    [Header("Physics / Layers & Tags")]
    [Tooltip("Layer used for both grass and lava colliders (should be in PlayerMovement.groundLayer).")]
    [SerializeField] private string groundLayerName = "Ground";
    [Tooltip("Tag for grass collider object (optional).")]
    [SerializeField] private string grassTag = "Ground";
    [Tooltip("Tag for lava collider object (MUST match PlayerDeath Obstacle).")]
    [SerializeField] private string lavaTag = "Obstacle";

    [Header("Debug / Seed")]
    [SerializeField] private bool useFixedSeed = false;
    [SerializeField] private int seed = 12345;

    // Internal
    private readonly List<PlatformSegmentMarker> segments = new List<PlatformSegmentMarker>();
    private float lastEndX;
    private float lastY;
    private int groundLayer;

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

        // Start at the end of your manual platform
        lastEndX = transform.position.x;
        lastY = transform.position.y;

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
        // Always extend to the right
        float gap = Random.Range(minGap, maxGap);
        float startX = lastEndX + gap;

        // Height change
        float offsetY = Random.Range(-maxStepDown, maxStepUp);
        float newY = Mathf.Clamp(lastY + offsetY, minY, maxY);

        // Core platform length (bez krawędzi)
        int coreCount = Random.Range(minTiles, maxTiles + 1);

        // Decide materials for core tiles (Grass / Lava)
        TileMaterial[] coreMaterials = GenerateCoreMaterials(coreCount);

        // Dodajemy 2 kafelki krawędziowe: lewy + prawy
        int visualCount = coreCount + 2;
        TileMaterial[] visualMaterials = new TileMaterial[visualCount];

        // Left boundary (index 0) - ten sam materiał co pierwszy kafel rdzenia
        visualMaterials[0] = coreMaterials[0];

        // Środek (1..visualCount-2) odpowiada core[0..coreCount-1]
        for (int ci = 0; ci < coreCount; ci++)
        {
            visualMaterials[ci + 1] = coreMaterials[ci];
        }

        // Right boundary (index last) - ten sam materiał co ostatni kafel rdzenia
        visualMaterials[visualCount - 1] = coreMaterials[coreCount - 1];

        float totalWidth = visualCount * tileWidth;

        // Parent for the whole segment (keep at origin, tiles at world positions)
        GameObject segmentGO = new GameObject("PlatformSegment");
        segmentGO.transform.parent = transform;
        segmentGO.transform.position = Vector3.zero;
        segmentGO.layer = groundLayer;

        // Static body
        Rigidbody2D rb2d = segmentGO.AddComponent<Rigidbody2D>();
        rb2d.bodyType = RigidbodyType2D.Static;
        rb2d.simulated = true;
        rb2d.gravityScale = 0f;

        // Marker for cleanup
        PlatformSegmentMarker marker = segmentGO.AddComponent<PlatformSegmentMarker>();
        marker.startX = startX;
        marker.endX = startX + totalWidth;

        // Collider holders
        GameObject grassColliderGO = new GameObject("GrassCollider");
        grassColliderGO.transform.parent = segmentGO.transform;
        grassColliderGO.transform.localPosition = Vector3.zero;
        grassColliderGO.transform.localRotation = Quaternion.identity;
        grassColliderGO.transform.localScale = Vector3.one;
        grassColliderGO.layer = groundLayer;
        if (!string.IsNullOrEmpty(grassTag))
            grassColliderGO.tag = grassTag;

        PolygonCollider2D grassPoly = grassColliderGO.AddComponent<PolygonCollider2D>();
        grassPoly.pathCount = 0;
        grassPoly.isTrigger = false;

        GameObject lavaColliderGO = new GameObject("LavaCollider");
        lavaColliderGO.transform.parent = segmentGO.transform;
        lavaColliderGO.transform.localPosition = Vector3.zero;
        lavaColliderGO.transform.localRotation = Quaternion.identity;
        lavaColliderGO.transform.localScale = Vector3.one;
        lavaColliderGO.layer = groundLayer;
        if (!string.IsNullOrEmpty(lavaTag))
            lavaColliderGO.tag = lavaTag;

        PolygonCollider2D lavaPoly = lavaColliderGO.AddComponent<PolygonCollider2D>();
        lavaPoly.pathCount = 0;
        lavaPoly.isTrigger = false;

        int grassPathIndex = 0;
        int lavaPathIndex = 0;

        // Create tiles and add shapes
        for (int i = 0; i < visualCount; i++)
        {
            float worldX = startX + i * tileWidth;
            float worldY = newY;

            GameObject tile = Instantiate(tilePrefab, segmentGO.transform);
            tile.transform.position = new Vector2(worldX, worldY);
            tile.layer = groundLayer;

            // Remove any collider from tile instance – colliders only on Grass/LavaCollider objects
            var tileCol = tile.GetComponent<Collider2D>();
            if (tileCol != null)
                Destroy(tileCol);

            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = tile.AddComponent<SpriteRenderer>();

            // Pick sprite based on visual materials and neighbors
            sr.sprite = ChooseSpriteForTile(visualMaterials, i, visualCount);

            Sprite sprite = sr.sprite;
            if (sprite == null)
                continue;

            bool isGrassTile = (visualMaterials[i] == TileMaterial.Grass);

            int shapeCount = sprite.GetPhysicsShapeCount();
            if (shapeCount == 0)
            {
                // fallback: full rect from bounds
                shapeBuffer.Clear();
                Bounds b = sprite.bounds;
                shapeBuffer.Add(new Vector2(b.min.x, b.min.y));
                shapeBuffer.Add(new Vector2(b.max.x, b.min.y));
                shapeBuffer.Add(new Vector2(b.max.x, b.max.y));
                shapeBuffer.Add(new Vector2(b.min.x, b.max.y));

                if (isGrassTile)
                    AddPhysicsPath(grassPoly, ref grassPathIndex, grassColliderGO.transform, tile.transform, shapeBuffer);
                else
                    AddPhysicsPath(lavaPoly, ref lavaPathIndex, lavaColliderGO.transform, tile.transform, shapeBuffer);
            }
            else
            {
                for (int s = 0; s < shapeCount; s++)
                {
                    shapeBuffer.Clear();
                    sprite.GetPhysicsShape(s, shapeBuffer);

                    if (isGrassTile)
                        AddPhysicsPath(grassPoly, ref grassPathIndex, grassColliderGO.transform, tile.transform, shapeBuffer);
                    else
                        AddPhysicsPath(lavaPoly, ref lavaPathIndex, lavaColliderGO.transform, tile.transform, shapeBuffer);
                }
            }
        }

        lastEndX = marker.endX;
        lastY = newY;
        segments.Add(marker);
    }

    /// <summary>
    /// Generates core tile materials for a single platform, ensuring:
    /// - at least one Grass overall
    /// - any Lava run is at least 2 tiles long
    /// </summary>
    private TileMaterial[] GenerateCoreMaterials(int coreCount)
    {
        var materials = new TileMaterial[coreCount];

        // Everything starts as grass
        for (int i = 0; i < coreCount; i++)
            materials[i] = TileMaterial.Grass;

        // Jeśli platforma jest za krótka albo wylosujemy brak lawy -> sama trawa
        int minRun = Mathf.Max(minLavaRun, 2); // wymuś min 2
        if (coreCount < minRun + 1 || Random.value > lavaChancePerPlatform)
            return materials;

        int maxRun = Mathf.Clamp(maxLavaRun, minRun, coreCount - 1); // zostaw przynajmniej 1 grass
        if (maxRun < minRun)
            return materials;

        int lavaRunLength = Random.Range(minRun, maxRun + 1);

        int startMax = coreCount - lavaRunLength; // żeby cały run się zmieścił
        int lavaStart = Random.Range(0, startMax + 1);
        int lavaEnd = lavaStart + lavaRunLength; // exclusive

        for (int i = lavaStart; i < lavaEnd; i++)
        {
            materials[i] = TileMaterial.Lava;
        }

        // mamy gwarancję, że coreCount - lavaRunLength >= 1 -> przynajmniej jeden Grass
        return materials;
    }

    /// <summary>
    /// Choose appropriate sprite based on visualMaterials (z boundary na indexach 0 i last).
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

        // KRAWĘDZIE PLATFORMY – zawsze jedna z: grass_left / grass_right / lava_left / lava_right
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

        // ŚRODEK
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

            // czysty odcinek trawy
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

            // czysty odcinek lawy
            if (!leftSame && rightSame)
                return lavaLeftSprite != null ? lavaLeftSprite : lavaMiddleSprite;
            if (leftSame && rightSame)
                return lavaMiddleSprite;
            if (leftSame && !rightSame)
                return lavaRightSprite != null ? lavaRightSprite : lavaMiddleSprite;

            return lavaMiddleSprite;
        }
    }

    private void AddPhysicsPath(
        PolygonCollider2D poly,
        ref int pathIndex,
        Transform colliderTransform,
        Transform tileTransform,
        List<Vector2> spriteLocalPoints)
    {
        if (spriteLocalPoints.Count < 2)
            return;

        Vector2[] path = new Vector2[spriteLocalPoints.Count];

        for (int i = 0; i < spriteLocalPoints.Count; i++)
        {
            // sprite-local -> world
            Vector3 world = tileTransform.TransformPoint(spriteLocalPoints[i]);
            // world -> collider-local
            Vector3 local = colliderTransform.InverseTransformPoint(world);
            path[i] = new Vector2(local.x, local.y);
        }

        poly.pathCount = pathIndex + 1;
        poly.SetPath(pathIndex, path);
        pathIndex++;
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
