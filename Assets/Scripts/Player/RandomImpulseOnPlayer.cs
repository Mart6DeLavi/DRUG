using UnityEngine;

public class RandomImpulseOnPlayer : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    private float nextImpulseTime = 0f;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        nextImpulseTime = 0f;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.randomImpulsActive) return;
        if (rb == null) return;

        if (Time.time >= nextImpulseTime)
        {
            DoRandomImpulse();

            if (GameManager.Instance.RandomImpulseUseRandomInterval)
            {
                Vector2 r = GameManager.Instance.RandomImpulseIntervalRange;
                float interval = Random.Range(Mathf.Min(r.x, r.y), Mathf.Max(r.x, r.y));
                nextImpulseTime = Time.time + Mathf.Max(0.02f, interval);
            }
            else
            {
                nextImpulseTime = Time.time + Mathf.Max(0.02f, GameManager.Instance.RandomImpulseInterval);
            }
        }
    }

    private void DoRandomImpulse()
    {
        float minF = GameManager.Instance.RandomImpulseMinForce;
        float maxF = GameManager.Instance.RandomImpulseMaxForce;

        float force = Random.Range(Mathf.Min(minF, maxF), Mathf.Max(minF, maxF));

        Vector2 dir = Random.insideUnitCircle.normalized;
        if (dir == Vector2.zero) dir = Vector2.right;

        rb.AddForce(dir * force, ForceMode2D.Impulse);
    }
}