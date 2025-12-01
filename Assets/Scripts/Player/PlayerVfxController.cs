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

    // Flaga: po skoku czekamy na pierwsze lądowanie
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
    /// Wywoływane, gdy PlayerMovement robi skok (OnJump event).
    /// </summary>
    private void HandleJump()
    {
        // Efekt skoku
        if (jumpEffect != null)
        {
            jumpEffect.transform.position = transform.position;
            jumpEffect.Play();
        }

        // Po skoku oczekujemy na pierwsze lądowanie
        pendingLandEffect = true;
    }

    /// <summary>
    /// Pierwsza kolizja z ziemią po skoku odpala efekt lądowania (tylko raz).
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!pendingLandEffect || landEffect == null)
            return;

        // Sprawdź, czy obiekt jest na warstwie groundLayer
        int otherLayerMask = 1 << collision.gameObject.layer;
        bool isGround = (otherLayerMask & groundLayer.value) != 0;

        if (!isGround)
            return;

        // Tu: pierwsze lądowanie po skoku
        pendingLandEffect = false;

        // Ustawiamy efekt w punkcie kontaktu lub przy stopach gracza
        Vector2 contactPoint = collision.GetContact(0).point;
        landEffect.transform.position = contactPoint;
        landEffect.Play();
    }

    /// <summary>
    /// Publiczna metoda do odpalenia efektu śmierci.
    /// Wywołaj ją z PlayerDeath.
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
