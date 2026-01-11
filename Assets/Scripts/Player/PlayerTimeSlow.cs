using UnityEngine;

public class PlayerTimeSlow : MonoBehaviour
{
    [Header("Time Slow Settings")]
    [Tooltip("Slow-down factor relative to base game tempo (0.2 = 20% speed).")]
    [Range(0.05f, 1f)]
    public float slowFactor = 0.2f;

    [Tooltip("Maximum slow-down time (in real-time seconds).")]
    public float maxSlowDuration = 5f;

    [Tooltip("Recharge speed (slow seconds per real second).")]
    public float rechargeRate = 1f;

    [Tooltip("If true, player has infinite slow-down time.")]
    public bool infiniteSlow = false;

    [Header("Debug / UI (view only)")]
    [Tooltip("Current slow-down resource state (0..maxSlowDuration).")]
    public float currentSlowTime;

    // Whether we are currently in slow motion mode
    private bool isSlowing = false;

    // Default fixedDeltaTime (Unity default is 0.02f)
    private const float defaultFixedDeltaTime = 0.02f;

    // Reference to PauseController to know baseTimeScale
    private PauseController pauseController;

    private void Start()
    {
        pauseController = FindFirstObjectByType<PauseController>();
        if (pauseController == null)
        {
            Debug.LogWarning("PlayerTimeSlow: PauseController not found in scene. " +
                             "Slow-down will be calculated relative to Time.timeScale = 1.");
        }

        currentSlowTime = maxSlowDuration;
    }

    private void Update()
    {
        // If game is paused (ESC) -> don't touch Time.timeScale,
        // but we can recharge the slow-down resource.
        if (Time.timeScale == 0f)
        {
            if (!infiniteSlow && currentSlowTime < maxSlowDuration)
            {
                currentSlowTime = Mathf.Min(
                    maxSlowDuration,
                    currentSlowTime + rechargeRate * Time.unscaledDeltaTime
                );
            }

            isSlowing = false;
            return;
        }

        // Shift (left or right)
        bool slowKeyHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (slowKeyHeld && (infiniteSlow || currentSlowTime > 0f))
        {
            // Slow-down enabled
            if (!isSlowing)
            {
                StartSlow();
            }

            if (!infiniteSlow)
            {
                // Resource usage counted in real time (unscaledDeltaTime)
                currentSlowTime = Mathf.Max(
                    0f,
                    currentSlowTime - Time.unscaledDeltaTime
                );

                // If resource ran out this frame
                if (currentSlowTime <= 0f)
                {
                    StopSlow();
                }
            }
        }
        else
        {
            // Key released -> return to normal tempo
            if (isSlowing)
            {
                StopSlow();
            }

            // Recharge resource when not using slow motion
            if (!infiniteSlow && currentSlowTime < maxSlowDuration)
            {
                currentSlowTime = Mathf.Min(
                    maxSlowDuration,
                    currentSlowTime + rechargeRate * Time.unscaledDeltaTime
                );
            }
        }
    }

    private void StartSlow()
    {
        isSlowing = true;

        float baseScale = 1f;
        if (pauseController != null)
        {
            baseScale = pauseController.baseTimeScale;
        }

        // Slow down relative to base tempo, but don't go to zero
        float targetScale = Mathf.Clamp(baseScale * slowFactor, 0.01f, baseScale);
        Time.timeScale = targetScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
    }

    private void StopSlow()
    {
        isSlowing = false;

        float baseScale = 1f;
        if (pauseController != null)
        {
            baseScale = pauseController.baseTimeScale;
        }

        // Return to base game tempo (e.g. 0.8)
        Time.timeScale = baseScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
    }
}
