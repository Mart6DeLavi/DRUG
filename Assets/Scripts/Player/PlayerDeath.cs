using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeathZone") || other.CompareTag("Obstacle"))
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player has died!");
        
        PlayerSounds sounds = GetComponent<PlayerSounds>();
        if (sounds != null)
            sounds.PlayDeathSound();

        if (SurvivalScore.Instance != null)
        {
            SurvivalScore.Instance.SealFinalScore();
        }

        SceneManager.LoadScene("GameOverScene");
    }
}
