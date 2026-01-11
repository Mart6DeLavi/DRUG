using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [Header("Death settings")]
    [Tooltip("Tag for instant death zone (e.g. bottom of map).")]
    public string deathZoneTag = "DeathZone";

    [Tooltip("Tag for lava / deadly obstacles.")]
    public string obstacleTag = "Obstacle";

    [Tooltip("If player moves upward faster than this value, collision with lava is ignored.")]
    public float upwardIgnoreVelocity = 0.1f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1) classic death zone – always kills
        if (other.CompareTag(obstacleTag))
        {
            Die();
            return;
        }

        // 2) lava / obstacles: kill only if not flying clearly upward
        if (other.CompareTag(deathZoneTag))
        {
            if (rb != null && rb.linearVelocity.y > upwardIgnoreVelocity)
            {
                // Jumping upward into lava from below -> ignore
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

        // death particle effect
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
