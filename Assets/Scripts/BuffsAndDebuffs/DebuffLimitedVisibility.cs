using UnityEngine;

public class DebuffLimitedVisibility : MonoBehaviour
{
    public float duration = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.ActivateLimitedVisibility(duration);
            Destroy(gameObject);
        }
    }
}
