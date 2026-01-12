using UnityEngine;

/// <summary>
/// Piła: jeździ lewo–prawo jako dziecko segmentu platformy i odbija się gdy:
/// - kończy się platforma (brak Tile_ pod spodem),
/// - pod spodem jest lawa (Tile_ ma tag Obstacle),
/// - przed nią jest inny trap (tag Obstacle).
/// Dodatkowo ma prostą animację sprite'ów (frames).
/// </summary>
[DisallowMultipleComponent]
public class SawBladePatrol : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 2.2f;
    [SerializeField] private int startDirection = 1; // 1 = prawo, -1 = lewo

    [Header("Ground check (platform / lava)")]
    [SerializeField] private float groundCheckDistance = 2.0f;
    [Tooltip("Podnosimy punkt sprawdzania żeby nie startować raycasta z wnętrza kolidera platformy.")]
    [SerializeField] private float groundProbeUpOffset = 0.2f;

    [Tooltip("Tag używany przez lavę i trapy (w generatorze lavaTag = \"Obstacle\").")]
    [SerializeField] private string obstacleTag = "Obstacle";

    [Tooltip("Tag używany przez lawę/strefę śmierci w projekcie.")]
    [SerializeField] private string deathZoneTag = "DeathZone";

    [Header("Edge padding")]
    [Tooltip("Ile wcześniej (w jednostkach świata) zawracać przed końcem platformy, żeby nie wjeżdżać na zaokrąglony rant.")]
    [SerializeField] private float edgeTurnPadding = 0.25f;

    [Header("Obstacle check (other traps)")]
    [SerializeField] private float forwardLookAhead = 0.08f;
    [SerializeField] private LayerMask obstacleMask = ~0;

    [Header("Sprite animation (2–3 frames)")]
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

        // promień do CircleCast (z collidra lub renderer-a)
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

        // local space (platformy/segmenty mogą się ruszać)
        Vector3 local = transform.localPosition;
        Vector3 nextLocal = local + new Vector3(step, 0f, 0f);

        Vector3 nextWorld = (_parent != null) ? _parent.TransformPoint(nextLocal) : nextLocal;

        // 1) Czy pod spodem nadal jest platforma? + zawracanie wcześniej przed krawędzią
        Vector3 edgeProbeWorld = nextWorld + new Vector3(_dir * Mathf.Max(0f, edgeTurnPadding), 0f, 0f);

        if (!HasSolidTileBelow(edgeProbeWorld, out Collider2D tileCol))
        {
            Flip();
            return;
        }

        // 2) Czy pod spodem nie jest lawa / deathzone?
        // (Obsługujemy zarówno przypadek gdy "lava" jest tilem (Tile_*) jak i gdy jest osobnym colliderem.)
        if (IsTagBelow(edgeProbeWorld, obstacleTag) || IsTagBelow(edgeProbeWorld, deathZoneTag))
        {
            Flip();
            return;
        }

        // 3) czy przed nami nie ma innego trapa?
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

            // kafelki platformy z generatora: Tile_0, Tile_1, ...
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

        // Dla deathzone/trapów chcemy też triggery (często są triggerami)
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