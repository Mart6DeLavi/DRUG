using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;          // Horizontal movement speed
    public float jumpForce = 7f;      // Jump strength

    [Header("Ground Detection")]
    public Transform groundCheck;     // Empty GameObject placed below the player
    public float groundCheckRadius = 0.15f; // Radius of the ground detection circle
    public LayerMask groundLayer;     // Layer assigned to ground objects

    private Rigidbody2D rb;           // Reference to Rigidbody2D
    private bool isGrounded;          // True if player is on the ground
    private float moveInput;          // Horizontal input value

    void Start()
    {
        // Get the Rigidbody2D component from the player object
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Read horizontal movement input (-1 for left, 1 for right)
        moveInput = Input.GetAxisRaw("Horizontal");

        // Jump only if the player is currently grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Set the upward velocity to jumpForce
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        // Move horizontally (keep existing vertical velocity)
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // Check if the player is standing on the ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // Optional: visualize the ground check circle in the Scene view
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
