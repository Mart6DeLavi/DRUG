using UnityEngine;

public class PlayerVfxController : MonoBehaviour
{
    [Header("Particle Systems")]
    [Tooltip("Effect played when the player jumps.")]
    public ParticleSystem jumpEffect;

    [Tooltip("Effect played when the player lands on the ground.")]
    public ParticleSystem landEffect;

    [Tooltip("Effect played when the player dies.")]
    public ParticleSystem deathEffect;

    [Header("Landing Detection")]
    [Tooltip("Which layers are considered ground for landing effects.")]
    public LayerMask groundLayer;

    private PlayerMovement playerMovement;
    private Rigidbody2D rb;

    // Flag: after jump we wait for first landing
    private bool pendingLandEffect = false;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        if (playerMovement != null)
        {
            playerMovement.OnJump += HandleJump;
        }
    }

    void OnDisable()
    {
        if (playerMovement != null)
        {
            playerMovement.OnJump -= HandleJump;
        }
    }

    /// <summary>
    /// Called when PlayerMovement performs a jump (OnJump event).
    /// </summary>
    private void HandleJump()
    {
        // Jump effect
        if (jumpEffect != null)
        {
            jumpEffect.transform.position = transform.position;
            jumpEffect.Play();
        }

        // After jump we await first landing
        pendingLandEffect = true;
    }

    /// <summary>
    /// First ground collision after jump triggers landing effect (only once).
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!pendingLandEffect || landEffect == null)
            return;

        // Check if object is on groundLayer
        int otherLayerMask = 1 << collision.gameObject.layer;
        bool isGround = (otherLayerMask & groundLayer.value) != 0;

        if (!isGround)
            return;

        // Here: first landing after jump
        pendingLandEffect = false;

        // Set effect at contact point or at player's feet
        Vector2 contactPoint = collision.GetContact(0).point;
        landEffect.transform.position = contactPoint;
        landEffect.Play();
    }

    /// <summary>
    /// Public method to trigger death effect.
    /// Call it from PlayerDeath.
    /// </summary>
    public void PlayDeathEffect()
    {
        if (deathEffect != null)
        {
            deathEffect.transform.position = transform.position;
            deathEffect.Play();
        }
    }
}
