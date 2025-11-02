using UnityEngine;
using System; // for Action

public class PlayerMovement : MonoBehaviour
{
    // >>> ADD THIS EVENT <<<
    public event Action OnJump; // Fired right after a successful jump

    [Header("Movement Settings")]
    public float speed = 5f;
    public float jumpForce = 7f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
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
        moveInput = Input.GetAxisRaw("Horizontal");

        // Jump only if grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // >>> FIRE THE EVENT HERE <<<
            OnJump?.Invoke();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
