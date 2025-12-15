using UnityEngine;

public class PlatformLedgeAssist : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public Collider2D playerCollider;
    public Transform probe; // Empty child; place on the RIGHT side near the player's mid height

    [Header("Layers")]
    public LayerMask platformLayer;

    [Header("Probe Shape (vertical line-like box)")]
    public float probeWidth = 0.06f;
    public float probeHeight = 1.0f;
    public float castDistance = 0.10f;

    [Header("Step / Snap")]
    public float maxStepUp = 0.45f;
    public float extraStepUp = 0.02f;
    public float stepUpSpeed = 40f; // increase for more instant snap

    [Header("Behaviour")]
    [Tooltip("Minimum horizontal INPUT needed to attempt stepping (0.1 is good).")]
    public float minTowardInput = 0.1f;

    [Tooltip("If true, don't step while moving upward (head-hit cases). Turn OFF if you want climb while rising.")]
    public bool requireFallingOrNeutralY = false;

    public bool debugDraw = false;

    [Tooltip("Show a small debug label in the Game view (useful when you can't watch Scene view).")]
    public bool debugHud = false;

    [Tooltip("Only attempt ledge assist while NOT grounded. Prevents stepping/bouncing when standing on a platform.")]
    public bool onlyWhileAirborne = true;

    [Tooltip("Extra distance for the simple ground raycast.")]
    public float groundedRayExtra = 0.05f;

    private float lastDirSign = 1f;

    private Collider2D _currentGround;
    private string _dbg;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (playerCollider == null) playerCollider = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (rb == null || playerCollider == null || probe == null) return;

        // Detect what we're currently standing on (if anything)
        _currentGround = GetCurrentGround();

        if (onlyWhileAirborne && _currentGround != null)
        {
            _dbg = "grounded (skip)";
            return;
        }

        if (requireFallingOrNeutralY && rb.linearVelocity.y > 0.05f)
            return;

        // Direction from INPUT (works even when body is stuck and vx ~ 0)
        float inputX = Input.GetAxisRaw("Horizontal");
        float dirSign;

        if (Mathf.Abs(inputX) >= minTowardInput)
        {
            dirSign = Mathf.Sign(inputX);
        }
        else
        {
            // fallback to velocity or last direction
            float vx = rb.linearVelocity.x;
            dirSign = Mathf.Abs(vx) > 0.01f ? Mathf.Sign(vx) : lastDirSign;
        }

        if (Mathf.Abs(dirSign) < 0.01f) return;
        lastDirSign = dirSign;

        Vector2 dir = new Vector2(dirSign, 0f);

        Bounds pb = playerCollider.bounds;
        Vector2 playerCenter = pb.center;

        // Probe world pos, mirrored to movement side
        Vector2 probeWorld = probe.position;
        float localX = probeWorld.x - playerCenter.x;
        if (Mathf.Sign(localX) != dirSign)
            probeWorld.x = playerCenter.x - localX;

        Vector2 boxSize = new Vector2(probeWidth, probeHeight);

        RaycastHit2D hit = Physics2D.BoxCast(probeWorld, boxSize, 0f, dir, castDistance, platformLayer);

        // If the probe hit the same collider we're standing on, ignore (prevents tiny step-ups / bouncing)
        if (_currentGround != null && hit.collider == _currentGround)
        {
            _dbg = "hit current ground (skip)";
            return;
        }

        _dbg = (hit.collider != null) ? $"hit: {hit.collider.name}" : "no hit";

        if (debugDraw)
        {
            Debug.DrawLine(probeWorld, probeWorld + dir * castDistance, Color.yellow, Time.fixedDeltaTime);
        }

        if (hit.collider == null) return;

        // Platform top and how much we need to lift so player bottom is above it
        float platformTopY = hit.collider.bounds.max.y;
        float playerBottomY = playerCollider.bounds.min.y;

        float neededUp = (platformTopY - playerBottomY) + extraStepUp;

        if (neededUp <= 0.001f || neededUp > maxStepUp)
            return;

        // Check that target space is free (avoid snapping into ceilings/walls)
        Vector2 targetCenter = new Vector2(playerCenter.x, playerCenter.y + neededUp);
        Vector2 overlapSize = pb.size * 0.95f;

        Collider2D overlap = Physics2D.OverlapBox(targetCenter, overlapSize, 0f, platformLayer);
        if (overlap != null && overlap != hit.collider)
            return;

        float nextY = Mathf.MoveTowards(rb.position.y, rb.position.y + neededUp, stepUpSpeed * Time.fixedDeltaTime);
        rb.position = new Vector2(rb.position.x, nextY);

        // Prevent "sticking" upward after step
        if (rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
    }

    private Collider2D GetCurrentGround()
    {
        if (playerCollider == null) return null;

        Bounds b = playerCollider.bounds;

        // Cast down a tiny bit below the player's feet
        Vector2 origin = new Vector2(b.center.x, b.min.y + 0.02f);
        float dist = groundedRayExtra;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, dist, platformLayer);
        return hit.collider;
    }

    private void OnGUI()
    {
        if (!debugHud) return;
        GUI.Label(new Rect(10, 10, 600, 22), $"[LedgeAssist] {_dbg}");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (probe == null) return;
        Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
        Gizmos.DrawWireCube(probe.position, new Vector3(probeWidth, probeHeight, 0f));
    }
#endif
}