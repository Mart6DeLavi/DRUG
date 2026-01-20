using UnityEngine;

public class RandomEffectPickup : MonoBehaviour
{
    [Header("Common")]
    public float duration = 5f;

    [Header("Buff params")]
    public float speedBoostMultiplier = 1.5f;
    public float slowPlayerMultiplier = 0.6f;

    [Header("Chances (weights)")]
    public int wSpeedBoost = 20;
    public int wDoubleJump = 20;
    public int wDoublePoints = 15;

    public int wReverseControls = 15;
    public int wRandomImpulse = 15;
    public int wSlowPlayer = 10;
    public int wLimitedVisibility = 5;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var gm = GameManager.Instance;
        if (gm == null) return;

        int total =
            wSpeedBoost + wDoubleJump + wDoublePoints +
            wReverseControls + wRandomImpulse + wSlowPlayer + wLimitedVisibility;

        int roll = UnityEngine.Random.Range(0, total);

        // BUFF: Speed
        roll -= wSpeedBoost;
        if (roll < 0)
        {
            gm.ActivateSpeedBoost(duration, speedBoostMultiplier);
            Destroy(gameObject);
            return;
        }

        // BUFF: DoubleJump
        roll -= wDoubleJump;
        if (roll < 0)
        {
            gm.ActivateDoubleJump(duration);
            Destroy(gameObject);
            return;
        }

        // BUFF: DoublePoints
        roll -= wDoublePoints;
        if (roll < 0)
        {
            gm.ActivateDoublePoints(duration);
            Destroy(gameObject);
            return;
        }

        // DEBUFF: Reverse controls
        roll -= wReverseControls;
        if (roll < 0)
        {
            gm.ActivateReverseControls(duration);
            Destroy(gameObject);
            return;
        }

        // DEBUFF: Random impulse
        roll -= wRandomImpulse;
        if (roll < 0)
        {
            gm.ActivateRandomImpulse(duration);
            Destroy(gameObject);
            return;
        }

        // DEBUFF: Slow player
        roll -= wSlowPlayer;
        if (roll < 0)
        {
            gm.ActivateSlowPlayer(duration, slowPlayerMultiplier);
            Destroy(gameObject);
            return;
        }

        // DEBUFF: Limited visibility
        gm.ActivateLimitedVisibility(duration);
        Destroy(gameObject);
    }
}
