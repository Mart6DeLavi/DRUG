using UnityEngine;
using System; // for Action

public class PlayerMovement : MonoBehaviour
{
    public event Action OnJump; // Fired right after a successful jump

    [Header("Movement Settings")]
    public float speed = 5f;          // Horizontal movement speed
    public float jumpForce = 7f;      // Jump strength
    public bool extraJumpAvailable = true; // For double jump tracking

    [Header("Ground Detection (multi-point)")]
    [Tooltip("Points under the player used to detect ground. E.g. Left, Center, Right.")]
    public Transform[] groundChecks;  // Multiple ground check points
    public float groundCheckRadius = 0.18f; // Radius for each check circle
    public LayerMask groundLayer;     // Layer of platforms (e.g. Ground)

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

        // reverse movment
        if (GameManager.Instance != null && GameManager.Instance.controlsReversed)
        {
            moveInput *= -1f;
        }

        bool canJump =
                isGrounded || // normal jump
                (GameManager.Instance != null && GameManager.Instance.doubleJumpActive && extraJumpAvailable);

        if (Input.GetButtonDown("Jump") && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            OnJump?.Invoke();

            if (!isGrounded && GameManager.Instance != null && GameManager.Instance.doubleJumpActive)
            {
                extraJumpAvailable = false; // using double jump
            }
        }
    }

    void FixedUpdate()
    {
        float multiplier = 1f;

        // USING GAME MANAGER TO ADJUST PLAYER SPEED FROM BONUSES
        if (GameManager.Instance != null)
        {
            multiplier = GameManager.Instance.playerSpeedMultiplier;
        }

        // Reset extra jump when grounded
        if (isGrounded)
        {
            extraJumpAvailable = true;
        }

        // Random impulse debuff
        if (GameManager.Instance != null && GameManager.Instance.randomImpulsActive)
        {
            if (UnityEngine.Random.value < 0.7f) // chance each FixedUpdate
            {
                float force = UnityEngine.Random.Range(-4f, 4f);
                rb.AddForce(new Vector2(force, 0), ForceMode2D.Impulse);
            }
        }

        // Horizontal movement (keep current vertical velocity)
        rb.linearVelocity = new Vector2(moveInput * speed * multiplier, rb.linearVelocity.y);

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
