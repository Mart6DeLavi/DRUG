using UnityEngine;

public class BonusDoubleJump : MonoBehaviour
{
    public float duration = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.ActivateDoubleJump(duration);
            Destroy(gameObject);
        }
    }
}
