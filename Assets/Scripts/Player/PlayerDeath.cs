using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerDeath : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // Chech if the player collides with an object tagged as "DeathZone"
        if (other.CompareTag("DeathZone") || other.CompareTag("Obstacle"))
        {
            Die();
        }

    }

    void Die()
    {
        Debug.Log("Player has died!");
        // Zapisz wynik czasu przetrwania zanim przełączymy scenę
        if (SurvivalScore.Instance != null)
        {
            SurvivalScore.Instance.SealFinalScore();
        }
        SceneManager.LoadScene("GameOverScene");
    }
}
