using UnityEngine;

public class BonusPoints : MonoBehaviour
{
    public float duration = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SurvivalScore.Instance.ActivatePointsMultiplier(2f, duration);
            Destroy(gameObject);
        }
    }
}
