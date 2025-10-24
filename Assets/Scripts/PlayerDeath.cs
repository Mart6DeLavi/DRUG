using UnityEngine;
using UnityEngine.SceneManagement;

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
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name); // Restart scene
    }
}
