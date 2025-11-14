using UnityEngine;

public class DebuffReverseControls : MonoBehaviour
{
    public float duration = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.ActivateReverseControls(duration);
            Destroy(gameObject);
        }
    }
}
