using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [Header("Death settings")]
    [Tooltip("Tag strefy natychmiastowej śmierci (np. dół mapy).")]
    public string deathZoneTag = "DeathZone";

    [Tooltip("Tag lawy / przeszkód śmiertelnych.")]
    public string obstacleTag = "Obstacle";

    [Tooltip("Jeśli gracz porusza się szybciej w górę niż ta wartość, zderzenie z lawą jest ignorowane.")]
    public float upwardIgnoreVelocity = 0.1f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1) klasyczna strefa śmierci – zawsze zabija
        if (other.CompareTag(obstacleTag))
        {
            Die();
            return;
        }

        // 2) lawa / przeszkody: zabijają tylko jeśli nie lecimy wyraźnie do góry
        if (other.CompareTag(deathZoneTag))
        {
            if (rb != null && rb.linearVelocity.y > upwardIgnoreVelocity)
            {
                // Skaczemy w górę w lawę od spodu -> ignorujemy
                return;
            }

            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player has died!");
        
        PlayerSounds sounds = GetComponent<PlayerSounds>();
        if (sounds != null)
            sounds.PlayDeathSound();

        // efekt cząsteczkowy śmierci
        PlayerVfxController vfx = GetComponent<PlayerVfxController>();
        if (vfx != null)
            vfx.PlayDeathEffect();
        // ----------------------------------------

        if (SurvivalScore.Instance != null)
        {
            SurvivalScore.Instance.SealFinalScore();
        }

        SceneManager.LoadScene("GameOverScene");
    }
}
