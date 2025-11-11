using System.Collections.Generic;
using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;        // Player to follow
    [SerializeField] private GameObject tilePrefab;   // Prefab with SpriteRenderer
    [SerializeField] private Sprite leftSprite;       // Left edge sprite
    [SerializeField] private Sprite middleSprite;     // Middle sprite
    [SerializeField] private Sprite rightSprite;      // Right edge sprite

    [Header("Tile Settings")]
    [SerializeField] private float tileWidth = 1f;    // Width of one tile in world units

    [Header("Platform Length (tiles)")]
    [SerializeField] private int minTiles = 2;
    [SerializeField] private int maxTiles = 8;

    [Header("Horizontal Gaps (world units)")]
    [SerializeField] private float minGap = 1.5f;
    [SerializeField] private float maxGap = 3.5f;

    [Header("Vertical Constraints")]
    [SerializeField] private float maxStepUp = 1.5f;
    [SerializeField] private float maxStepDown = 3f;
    [SerializeField] private float minY = -3f;
    [SerializeField] private float maxY = 5f;

    [Header("Generation Range")]
    [SerializeField] private float aheadDistance = 25f;
    [SerializeField] private float behindDistance = 20f;

    [Header("Physics / Layers")]
    [Tooltip("Layer used for platforms (must match PlayerMovement groundLayer).")]
    [SerializeField] private string groundLayerName = "Ground";

    [Header("Debug / Seed")]
    [SerializeField] private bool useFixedSeed = false;
    [SerializeField] private int seed = 12345;

    // Internal
    private readonly List<PlatformSegmentMarker> segments = new List<PlatformSegmentMarker>();
    private float lastEndX;
    private float lastY;
    private int groundLayer;

    // Reusable list to avoid GC
    private readonly List<Vector2> shapeBuffer = new List<Vector2>(32);

    private void Awake()
    {
        if (useFixedSeed)
            Random.InitState(seed);

        groundLayer = LayerMask.NameToLayer(groundLayerName);
        if (groundLayer < 0)
        {
            Debug.LogError($"PlatformGenerator: Layer '{groundLayerName}' does not exist. Create it and assign to ground.");
            groundLayer = 0; // Fallback: Default
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
            Debug.LogError("PlatformGenerator: Tile Prefab is not assigned.");
            enabled = false;
            return;
        }

        if (leftSprite == null || middleSprite == null || rightSprite == null)
        {
            Debug.LogWarning("PlatformGenerator: One or more platform sprites are not assigned.");
        }

        // Start at the end of your manual start platform (set this object's X there)
        lastEndX = transform.position.x;
        lastY = transform.position.y;

        // Initial platforms
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

        // New Y within jumpable/allowed range
        float offsetY = Random.Range(-maxStepDown, maxStepUp);
        float newY = Mathf.Clamp(lastY + offsetY, minY, maxY);

        // Length in tiles
        int tilesCount = Random.Range(minTiles, maxTiles + 1);
        if (tilesCount < 2) tilesCount = 2;

        float totalWidth = tilesCount * tileWidth;

        // Parent for the segment (stay at world origin for simpler math)
        GameObject segmentGO = new GameObject("PlatformSegment");
        segmentGO.transform.parent = transform;
        segmentGO.transform.position = Vector3.zero;
        segmentGO.layer = groundLayer;

        // Static body for physics
        var rb2d = segmentGO.AddComponent<Rigidbody2D>();
        rb2d.bodyType = RigidbodyType2D.Static;
        rb2d.simulated = true;
        rb2d.gravityScale = 0f;

        // Polygon collider that will be built from sprites' physics shapes
        var poly = segmentGO.AddComponent<PolygonCollider2D>();
        poly.pathCount = 0;
        poly.isTrigger = false;

        // Marker for cleanup
        var marker = segmentGO.AddComponent<PlatformSegmentMarker>();
        marker.startX = startX;
        marker.endX = startX + totalWidth;

        int pathIndex = 0;

        // Create tiles (visual only) and add their shapes into polygon collider
        for (int i = 0; i < tilesCount; i++)
        {
            float worldX = startX + i * tileWidth;
            float worldY = newY;

            GameObject tile = Instantiate(tilePrefab, segmentGO.transform);
            tile.transform.position = new Vector2(worldX, worldY);
            tile.layer = groundLayer;

            // Make sure no tile collider participates in physics directly
            var tileCol = tile.GetComponent<Collider2D>();
            if (tileCol != null)
                Destroy(tileCol);

            var sr = tile.GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = tile.AddComponent<SpriteRenderer>();

            // Assign correct sprite
            if (i == 0)
                sr.sprite = leftSprite != null ? leftSprite : middleSprite;
            else if (i == tilesCount - 1)
                sr.sprite = rightSprite != null ? rightSprite : middleSprite;
            else
                sr.sprite = middleSprite;

            var sprite = sr.sprite;
            if (sprite == null)
                continue;

            int shapeCount = sprite.GetPhysicsShapeCount();

            if (shapeCount == 0)
            {
                // Fallback: rectangle based on sprite bounds if no physics shape is defined
                shapeBuffer.Clear();
                var b = sprite.bounds; // local in sprite space
                shapeBuffer.Add(new Vector2(b.min.x, b.min.y));
                shapeBuffer.Add(new Vector2(b.max.x, b.min.y));
                shapeBuffer.Add(new Vector2(b.max.x, b.max.y));
                shapeBuffer.Add(new Vector2(b.min.x, b.max.y));

                AddPhysicsPathFromSpriteShape(poly, ref pathIndex, tile.transform, segmentGO.transform, shapeBuffer);
            }
            else
            {
                // Use all physics shapes defined on this sprite
                for (int s = 0; s < shapeCount; s++)
                {
                    shapeBuffer.Clear();
                    sprite.GetPhysicsShape(s, shapeBuffer);
                    AddPhysicsPathFromSpriteShape(poly, ref pathIndex, tile.transform, segmentGO.transform, shapeBuffer);
                }
            }
        }

        lastEndX = marker.endX;
        lastY = newY;
        segments.Add(marker);
    }

    /// <summary>
    /// Converts points from sprite-local (physics shape) via tile transform into segment-local,
    /// and appends them as a new path in the PolygonCollider2D.
    /// </summary>
    private void AddPhysicsPathFromSpriteShape(PolygonCollider2D poly, ref int pathIndex,
                                               Transform tileTransform, Transform segmentTransform,
                                               List<Vector2> spriteLocalPoints)
    {
        if (spriteLocalPoints.Count < 2)
            return;

        var path = new Vector2[spriteLocalPoints.Count];

        for (int i = 0; i < spriteLocalPoints.Count; i++)
        {
            // from sprite-local (pivot) to world
            Vector3 world = tileTransform.TransformPoint(spriteLocalPoints[i]);
            // to segment-local
            Vector3 localToSegment = segmentTransform.InverseTransformPoint(world);
            path[i] = new Vector2(localToSegment.x, localToSegment.y);
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
