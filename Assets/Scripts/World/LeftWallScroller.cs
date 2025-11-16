using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls a left boundary wall that moves to the right and accelerates over time.
/// Final speed is calculated as:
///   baseScrollSpeed * GameSpeedController.Instance.CurrentMultiplier * TempoEffectController.Instance.CurrentTempoMultiplier
/// The speed is clamped by maxScrollSpeed.
/// If the wall touches the Player (tag "Player"), triggers a Game Over.
/// 
/// Attach this to any GameObject representing the left boundary; ensure it has a 2D collider
/// (BoxCollider2D recommended). Use a Kinematic Rigidbody2D if you want physics-friendly movement.
/// </summary>
public class LeftWallScroller : MonoBehaviour   
{
    [Header("Movement Settings")]
    [Tooltip("Base movement speed in world units per second.")]
    [Min(0f)] public float baseScrollSpeed = 2f;

    [Tooltip("Maximum allowed scroll speed (0 = unlimited).")]
    [Min(0f)] public float maxScrollSpeed = 12f;

    [Tooltip("Acceleration rate in world units per second squared (how quickly wallSpeed changes).")]
    [Min(0f)] public float accelerationRate = 4f;

    [Tooltip("Multiplier applied to accelerationRate when wall is far behind the player.")]
    [Min(1f)] public float boostAccelerationMultiplier = 3f;

    [Tooltip("Distance threshold (player.x - wall.x) beyond which boosted acceleration applies.")]
    [Min(0f)] public float distanceBoostThreshold = 8f;

    [Tooltip("Optional small growth rate for baseScrollSpeed over time (units per second).")]
    [Min(0f)] public float baseGrowthRate = 0.2f;

    [Tooltip("Additional cap: wall will not exceed this speed (0 = no additional cap). Set slightly below player's speed.")]
    [Min(0f)] public float maxWallSpeed = 0f;

    // Internal, smoothed speed that approaches the target using accelerationRate.
    private float wallSpeed = 0f;

    [Header("Collision/Game Over")]
    [Tooltip("Tag used to identify the player object.")]
    public string playerTag = "Player";

    void Update()
    {
        // 1) Gather multipliers from global controllers; fall back to 1 if missing
        float globalRamp = 1f;
        if (GameSpeedController.Instance != null)
            globalRamp = GameSpeedController.Instance.CurrentMultiplier;

        float tempo = 1f;
        if (TempoEffectController.Instance != null)
            tempo = TempoEffectController.Instance.CurrentTempoMultiplier;

        // 2) Optional small growth of the base scroll speed (still capped later)
        if (baseGrowthRate > 0f)
        {
            baseScrollSpeed += baseGrowthRate * Time.deltaTime;
        }

        // 3) Compute desired target speed from existing logic
        float desiredSpeed = baseScrollSpeed * globalRamp * tempo;

        // 4) Determine effective cap so the wall never moves too fast
        //    - Keep previous maxScrollSpeed cap (if set)
        //    - Apply new maxWallSpeed cap (user should set slightly below player's speed)
        float effectiveMax = desiredSpeed;
        if (maxScrollSpeed > 0f) effectiveMax = Mathf.Min(effectiveMax, maxScrollSpeed);
        if (maxWallSpeed  > 0f) effectiveMax = Mathf.Min(effectiveMax, maxWallSpeed);

        // 5) Decide effective acceleration (boost if far behind player)
        float effectiveAcceleration = accelerationRate;
        if (boostAccelerationMultiplier > 1f && distanceBoostThreshold > 0f)
        {
            // Find player position (simple lookup by tag each frame; could be cached)
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                float distanceBehind = player.transform.position.x - transform.position.x;
                if (distanceBehind > distanceBoostThreshold)
                {
                    effectiveAcceleration *= boostAccelerationMultiplier;
                }
            }
        }

        // 6) Smoothly approach the (possibly capped) desired speed using (possibly boosted) acceleration
        wallSpeed = Mathf.MoveTowards(wallSpeed, effectiveMax, effectiveAcceleration * Time.deltaTime);

        // 7) Move the wall right using Time.deltaTime for smooth, frame-rate independent motion
        transform.position += Vector3.right * (wallSpeed * Time.deltaTime);
    }

    // 4) Handle trigger collision with Player (if collider set to IsTrigger)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && other.CompareTag(playerTag))
        {
            TriggerGameOver();
        }
    }

    // 5) Handle non-trigger collision with Player (if collider is not a trigger)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null && collision.collider != null && collision.collider.CompareTag(playerTag))
        {
            TriggerGameOver();
        }
    }

    /// <summary>
    /// Triggers the game over sequence. Seals survival score if available, then loads GameOverScene.
    /// </summary>
    void TriggerGameOver()
    {
        // If there is a survival score tracker, seal it before leaving the scene
        if (SurvivalScore.Instance != null)
        {
            SurvivalScore.Instance.SealFinalScore();
        }

        // Load the Game Over scene. Make sure the scene exists in Build Settings.
        SceneManager.LoadScene("GameOverScene");
    }
}
