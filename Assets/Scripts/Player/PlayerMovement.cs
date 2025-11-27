using UnityEngine;
using System; // for Action

public class PlayerMovement : MonoBehaviour
{
    public event Action OnJump; // Fired right after a successful jump

    [Header("Movement Settings")]
    [Tooltip("Base horizontal movement speed (used to compute max run speed).")]
    public float speed = 5f;

    [Tooltip("Strength of jump in direction of the mouse.")]
    public float jumpForce = 7f;

    [Tooltip("For double jump tracking (used with GameManager.doubleJumpActive).")]
    public bool extraJumpAvailable = true;

    [Header("Movement Physics")]
    [Tooltip("How strongly input (A/D or arrows) accelerates the player.")]
    public float moveAcceleration = 25f;

    [Tooltip("Maximum horizontal speed (before multipliers).")]
    public float maxHorizontalSpeed = 8f;

    [Header("Jump Direction Settings")]
    [Tooltip("Minimal upward component for jump direction (so you never jump strictly down).")]
    public float minUpwardY = 0.2f;

    [Header("Ground Detection (multi-point)")]
    [Tooltip("Points under the player used to detect ground. E.g. Left, Center, Right.")]
    public Transform[] groundChecks;  // Multiple ground check points

    [Tooltip("Radius of each ground-check circle.")]
    public float groundCheckRadius = 0.18f;

    [Tooltip("Layer of platforms (e.g. Ground).")]
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Read horizontal input (-1, 0, 1)
        moveInput = Input.GetAxisRaw("Horizontal");

        // Reverse movement if debuff is active
        if (GameManager.Instance != null && GameManager.Instance.controlsReversed)
        {
            moveInput *= -1f;
        }

        // Can we jump? (normal jump from ground OR double jump in the air if active)
        bool canJump =
            isGrounded || // normal jump
            (GameManager.Instance != null && GameManager.Instance.doubleJumpActive && extraJumpAvailable);

        if (Input.GetButtonDown("Jump") && canJump)
        {
            Vector2 dir;

            if (Camera.main != null)
            {
                // Mouse position in world space
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 rawDir = (Vector2)(mouseWorld - transform.position);

                // If mouse is exactly on player -> jump straight up
                if (rawDir.sqrMagnitude < 0.0001f)
                    rawDir = Vector2.up;

                // Force some upward component
                if (rawDir.y < minUpwardY)
                    rawDir.y = minUpwardY;

                dir = rawDir.normalized;
            }
            else
            {
                // Fallback if no camera
                dir = Vector2.up;
            }

            // Nadajemy nową prędkość w kierunku myszy (x i y)
            rb.linearVelocity = dir * jumpForce;

            // Event do dźwięku skoku
            OnJump?.Invoke();

            // Jeśli skaczemy w powietrzu i double jump jest aktywny -> wykorzystaj dodatkowy skok
            if (!isGrounded && GameManager.Instance != null && GameManager.Instance.doubleJumpActive)
            {
                extraJumpAvailable = false;
            }
        }
    }

    void FixedUpdate()
    {
        float multiplier = 1f;
        float globalSpeedMultiplier = 1f;
        float tempoMultiplier = 1f;

        // USING GAME MANAGER TO ADJUST PLAYER SPEED FROM BONUSES
        if (GameManager.Instance != null)
        {
            multiplier = GameManager.Instance.playerSpeedMultiplier;
        }

        // Global ramping speed (scroll) over time
        if (GameSpeedController.Instance != null)
        {
            globalSpeedMultiplier = GameSpeedController.Instance.CurrentMultiplier;
        }

        // Smooth tempo effect overlay
        if (TempoEffectController.Instance != null)
        {
            tempoMultiplier = TempoEffectController.Instance.CurrentTempoMultiplier;
        }

        // Reset extra jump when grounded
        if (isGrounded)
        {
            extraJumpAvailable = true;
        }

        // --- RUCH LEWO/PRAWO NA SIŁACH ---

        // Docelowa „efektywna” max prędkość pozioma (z buffami/debuffami)
        float effectiveMaxSpeed = maxHorizontalSpeed * multiplier * globalSpeedMultiplier * tempoMultiplier;

        // Siła przyspieszająca (proporcjonalna do wejścia i multiplikatorów)
        float effectiveAcceleration = moveAcceleration * multiplier * globalSpeedMultiplier * tempoMultiplier;

        // AddForce na osi X – działa jak „gaz”, a nie natychmiastowe ustawienie prędkości
        Vector2 force = Vector2.right * (moveInput * effectiveAcceleration);
        rb.AddForce(force, ForceMode2D.Force);

        // Ograniczamy prędkość poziomą do effectiveMaxSpeed
        Vector2 vel = rb.linearVelocity;
        if (Mathf.Abs(vel.x) > effectiveMaxSpeed)
        {
            vel.x = Mathf.Sign(vel.x) * effectiveMaxSpeed;
        }

        rb.linearVelocity = new Vector2(vel.x, rb.linearVelocity.y);

        // Random impulse debuff – dodatkowe losowe „kopnięcia”
        if (GameManager.Instance != null && GameManager.Instance.randomImpulsActive)
        {
            if (UnityEngine.Random.value < 0.7f) // chance each FixedUpdate
            {
                float forceImpulse = UnityEngine.Random.Range(-4f, 4f);
                rb.AddForce(new Vector2(forceImpulse, 0), ForceMode2D.Impulse);
            }
        }

        // Update grounded state using multiple check points
        isGrounded = IsGroundedMultiPoint();
    }

    /// <summary>
    /// Returns true if ANY of the groundCheck points touches the ground layer.
    /// </summary>
    private bool IsGroundedMultiPoint()
    {
        if (groundChecks != null && groundChecks.Length > 0)
        {
            foreach (Transform point in groundChecks)
            {
                if (point == null) continue;

                // Check small circle at this point
                bool hit = Physics2D.OverlapCircle(point.position, groundCheckRadius, groundLayer);
                if (hit)
                    return true;
            }
        }

        // Fallback: if no points are assigned, check from player's position (optional safety)
        Vector2 fallbackPos = transform.position;
        return Physics2D.OverlapCircle(fallbackPos, groundCheckRadius * 0.75f, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        if (groundChecks == null) return;

        Gizmos.color = Color.yellow;

        // Draw circles in Scene view so you see where checks are
        foreach (Transform point in groundChecks)
        {
            if (point == null) continue;
            Gizmos.DrawWireSphere(point.position, groundCheckRadius);
        }
    }
}
