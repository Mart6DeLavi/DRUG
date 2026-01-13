using UnityEngine;

/// <summary>
/// Saw blade: patrols left/right as a child of a platform segment and turns around when:
/// - the platform ends (no Tile_* below),
/// - lava / death zone is detected below,
/// - another trap is detected in front (Obstacle tag).
/// Also plays a simple sprite animation (frames).
/// </summary>
[DisallowMultipleComponent]
public class SawBladePatrol : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 2.2f;
    [SerializeField] private int startDirection = 1; // 1 = right, -1 = left

    [Header("Ground check (platform / lava)")]
    [SerializeField] private float groundCheckDistance = 2.0f;
    [Tooltip("Raise the probe origin so we don't raycast from inside the platform collider.")]
    [SerializeField] private float groundProbeUpOffset = 0.2f;

    [Tooltip("Tag used by traps (and optionally lava in older setups).")]
    [SerializeField] private string obstacleTag = "Obstacle";

    [Tooltip("Tag used by lava / death zone in this project.")]
    [SerializeField] private string deathZoneTag = "DeathZone";

    [Header("Edge padding")]
    [Tooltip("How early (world units) to turn around before the platform edge (to avoid the rounded end).")]
    [SerializeField] private float edgeTurnPadding = 0.25f;

    [Header("Obstacle check (other traps)")]
    [SerializeField] private float forwardLookAhead = 0.08f;
    [SerializeField] private LayerMask obstacleMask = ~0;

    [Header("Sprite animation (2-3 frames)")]
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 10f;
    [SerializeField] private bool randomStartFrame = true;

    private int _dir;
    private Transform _parent;
    private Collider2D _selfCollider;
    private float _radius;

    private SpriteRenderer _sr;
    private float _animT;
    private int _frameI;

    private void Awake()
    {
        _parent = transform.parent;
        _selfCollider = GetComponent<Collider2D>();

        _dir = (startDirection >= 0) ? 1 : -1;

        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();

        // CircleCast radius (from collider or renderer)
        _radius = 0.18f;
        if (_selfCollider != null)
            _radius = Mathf.Max(_radius, _selfCollider.bounds.extents.x);
        if (_sr != null)
            _radius = Mathf.Max(_radius, _sr.bounds.extents.x);

        // init anim
        if (frames != null && frames.Length > 0)
        {
            _frameI = randomStartFrame ? Random.Range(0, frames.Length) : 0;
            _sr.sprite = frames[_frameI];
        }
    }

    private void Update()
    {
        AnimateSprite();

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        float step = speed * dt * _dir;

        // Local space (platform/segments may move)
        Vector3 local = transform.localPosition;
        Vector3 nextLocal = local + new Vector3(step, 0f, 0f);

        Vector3 nextWorld = (_parent != null) ? _parent.TransformPoint(nextLocal) : nextLocal;

        // 1) Is there still platform below? + early turn before the edge
        Vector3 edgeProbeWorld = nextWorld + new Vector3(_dir * Mathf.Max(0f, edgeTurnPadding), 0f, 0f);

        if (!HasSolidTileBelow(edgeProbeWorld, out Collider2D tileCol))
        {
            Flip();
            return;
        }

        // 2) Czy pod spodem nie jest lawa / deathzone?
        // (Handles both cases: lava as a tile (Tile_*) or as a separate collider.)
        if (IsTagBelow(edgeProbeWorld, obstacleTag) || IsTagBelow(edgeProbeWorld, deathZoneTag))
        {
            Flip();
            return;
        }

        // 3) Is there another trap in front of us?
        if (HitsOtherTrapAhead())
        {
            Flip();
            return;
        }

        transform.localPosition = nextLocal;
    }

    private void AnimateSprite()
    {
        if (frames == null || frames.Length < 2) return;

        float frameTime = 1f / Mathf.Max(1f, fps);
        _animT += Time.deltaTime;

        while (_animT >= frameTime)
        {
            _animT -= frameTime;
            _frameI = (_frameI + 1) % frames.Length;
            _sr.sprite = frames[_frameI];
        }
    }

    private void Flip()
    {
        _dir *= -1;
    }

    private bool HasSolidTileBelow(Vector3 worldPos, out Collider2D tileCollider)
    {
        tileCollider = null;

        Vector3 origin = worldPos + Vector3.up * groundProbeUpOffset;

        ContactFilter2D filter = new ContactFilter2D
        {
            useTriggers = false,
            useLayerMask = true,
            layerMask = ~0
        };

        RaycastHit2D[] hits = new RaycastHit2D[6];
        int count = Physics2D.Raycast(origin, Vector2.down, filter, hits, groundCheckDistance);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = hits[i].collider;
            if (col == null) continue;

            // Platform tiles generated as: Tile_0, Tile_1, ...
            if (col.gameObject.name.StartsWith("Tile_"))
            {
                tileCollider = col;
                return true;
            }
        }

        return false;
    }

    private bool HitsOtherTrapAhead()
    {
        Vector2 origin = transform.position;
        Vector2 dir = new Vector2(_dir, 0f);

        float dist = _radius + forwardLookAhead;

        ContactFilter2D filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = true,
            layerMask = obstacleMask
        };

        RaycastHit2D[] hits = new RaycastHit2D[10];
        int count = Physics2D.CircleCast(origin, _radius, dir, filter, hits, dist);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = hits[i].collider;
            if (col == null) continue;
            if (_selfCollider != null && col == _selfCollider) continue;

            // ignoruj kafelki platformy
            if (col.gameObject.name.StartsWith("Tile_"))
                continue;

            if (col.gameObject.CompareTag(obstacleTag))
                return true;
        }

        return false;
    }

    private bool IsTagBelow(Vector3 worldPos, string tag)
    {
        if (string.IsNullOrEmpty(tag))
            return false;

        Vector3 origin = worldPos + Vector3.up * groundProbeUpOffset;

        // For DeathZone/traps we also want to detect triggers (they are often triggers)
        ContactFilter2D filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = true,
            layerMask = ~0
        };

        RaycastHit2D[] hits = new RaycastHit2D[8];
        int count = Physics2D.Raycast(origin, Vector2.down, filter, hits, groundCheckDistance);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = hits[i].collider;
            if (col == null) continue;

            // Ignore this saw's own collider (and any colliders on its children), otherwise it would detect itself as Obstacle.
            if (_selfCollider != null && col == _selfCollider)
                continue;
            if (col.transform == transform || col.transform.IsChildOf(transform))
                continue;

            if (col.gameObject.CompareTag(tag))
                return true;
        }

        return false;
    }
}
