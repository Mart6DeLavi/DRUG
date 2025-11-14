using UnityEngine;

public class BonusSpeed : MonoBehaviour
{
    public float speedMultiplier = 1.5f; // how much to multiply the player's speed
    public float duration = 5f;           // time duration of the speed boost in seconds; if 0 or less, the bonus is permanent

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (duration > 0)
                GameManager.Instance.ActivateSpeedBoost(duration, speedMultiplier);
            else
                GameManager.Instance.playerSpeedMultiplier = speedMultiplier; // bonus permanentny

            Destroy(gameObject); // destroy the bonus item after pickup
        }
    }
}
