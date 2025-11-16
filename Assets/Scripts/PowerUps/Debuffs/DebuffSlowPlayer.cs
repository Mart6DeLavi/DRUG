using UnityEngine;

public class DebuffSlowPlayer : MonoBehaviour
{
    public float duration = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.ActivateSlowPlayer(duration, 0.5f);
            Destroy(gameObject);
        }
    }
}
