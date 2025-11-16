using System.Collections;
using UnityEngine;

/// <summary>
/// Smooth acceleration (tempo) effect overlay.
/// Applies a temporary, smooth multiplier that eases up, holds, and eases down.
/// Multiplies with other speed factors (e.g., GameSpeedController, GameManager).
/// </summary>
public class TempoEffectController : MonoBehaviour
{
    public static TempoEffectController Instance { get; private set; }

    [Header("Defaults")]
    [Tooltip("Base tempo multiplier when idle.")]
    [Min(0f)] public float baseMultiplier = 1f;

    [Tooltip("Easing curve for rise/fall (0..1 time -> 0..1 value).")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Debug")]
    public bool logTransitions = false;

    public float CurrentTempoMultiplier { get; private set; } = 1f;

    Coroutine _activeRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ResetTempo();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void ResetTempo()
    {
        CurrentTempoMultiplier = Mathf.Max(0f, baseMultiplier);
        if (_activeRoutine != null)
        {
            StopCoroutine(_activeRoutine);
            _activeRoutine = null;
        }
    }

    /// <summary>
    /// Triggers a smooth acceleration effect:
    /// - Ease from current value to peakMultiplier over riseTime
    /// - Hold for holdTime
    /// - Ease back to baseMultiplier over fallTime
    /// Any active effect is interrupted and replaced by the new one, starting from the current value.
    /// </summary>
    /// <param name="peakMultiplier">Target peak multiplier (>= baseMultiplier)</param>
    /// <param name="riseTime">Seconds to ease-in</param>
    /// <param name="holdTime">Seconds to hold at peak</param>
    /// <param name="fallTime">Seconds to ease-out</param>
    public void TriggerBoost(float peakMultiplier, float riseTime, float holdTime, float fallTime)
    {
        peakMultiplier = Mathf.Max(baseMultiplier, peakMultiplier);
        if (_activeRoutine != null)
        {
            StopCoroutine(_activeRoutine);
        }
        _activeRoutine = StartCoroutine(RunBoostRoutine(peakMultiplier, Mathf.Max(0f, riseTime), Mathf.Max(0f, holdTime), Mathf.Max(0f, fallTime)));
    }

    IEnumerator RunBoostRoutine(float peak, float rise, float hold, float fall)
    {
        float from = CurrentTempoMultiplier;
        if (logTransitions)
            Debug.Log($"Tempo: rise from {from:F2} -> {peak:F2} in {rise:F2}s");

        // RISE
        if (rise > 0f)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / rise;
                float e = (easeCurve != null) ? easeCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
                CurrentTempoMultiplier = Mathf.Lerp(from, peak, e);
                yield return null;
            }
        }
        else
        {
            CurrentTempoMultiplier = peak;
            yield return null;
        }

        // HOLD
        if (hold > 0f)
        {
            float elapsed = 0f;
            while (elapsed < hold)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // FALL
        if (logTransitions)
            Debug.Log($"Tempo: fall to {baseMultiplier:F2} in {fall:F2}s");

        if (fall > 0f)
        {
            float start = CurrentTempoMultiplier;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / fall;
                float e = (easeCurve != null) ? easeCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
                CurrentTempoMultiplier = Mathf.Lerp(start, baseMultiplier, e);
                yield return null;
            }
        }
        else
        {
            CurrentTempoMultiplier = baseMultiplier;
            yield return null;
        }

        _activeRoutine = null;
    }
}
