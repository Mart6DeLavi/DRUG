using UnityEngine;

public class DebuffRandomImpulse : MonoBehaviour
{
    public float duration = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.ActivateRandomImpulse(duration);
            Destroy(gameObject);
        }
    }
}
