using UnityEngine;

public class GameSpeedController : MonoBehaviour
{
    public static GameSpeedController Instance { get; private set; }

    [Header("Ramping Settings")]
    [Tooltip("Starting speed multiplier.")]
    [Min(0f)] public float startMultiplier = 1f;

    [Tooltip("Maximum speed multiplier.")]
    [Min(0f)] public float maxMultiplier = 2.5f;

    [Tooltip("How fast the multiplier grows per second (linear).")]
    [Min(0f)] public float accelerationPerSecond = 0.05f;

    [Header("Optional Curve Override")]
    [Tooltip("If assigned, uses curve(Time.timeSinceLevelLoad) to determine multiplier, then clamps to [start,max]. Time axis is in seconds.")]
    public AnimationCurve customCurve;

    [Header("Debug UI (optional)")]
    public bool logSpeedChanges = false;

    public float CurrentMultiplier { get; private set; }

    float _lastLoggedValue;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ResetSpeed();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void ResetSpeed()
    {
        CurrentMultiplier = Mathf.Max(0f, startMultiplier);
        _lastLoggedValue = CurrentMultiplier;
    }

    void Update()
    {
        // If a custom curve is provided, evaluate it; otherwise ramp linearly.
        if (customCurve != null && customCurve.keys != null && customCurve.keys.Length > 0)
        {
            float curveValue = customCurve.Evaluate(Time.timeSinceLevelLoad);
            CurrentMultiplier = Mathf.Clamp(curveValue, Mathf.Min(startMultiplier, maxMultiplier), Mathf.Max(startMultiplier, maxMultiplier));
        }
        else
        {
            if (CurrentMultiplier < maxMultiplier)
            {
                CurrentMultiplier += accelerationPerSecond * Time.deltaTime;
                if (CurrentMultiplier > maxMultiplier)
                    CurrentMultiplier = maxMultiplier;
            }
        }

        if (logSpeedChanges && Mathf.Abs(CurrentMultiplier - _lastLoggedValue) > 0.05f)
        {
            Debug.Log($"GameSpeedController multiplier: {CurrentMultiplier:F2}");
            _lastLoggedValue = CurrentMultiplier;
        }
    }
}
