using UnityEngine;

public class PlayerTimeSlow : MonoBehaviour
{
    [Header("Time Slow Settings")]
    [Tooltip("Współczynnik spowolnienia względem bazowego tempa gry (0.2 = 20% prędkości).")]
    [Range(0.05f, 1f)]
    public float slowFactor = 0.2f;

    [Tooltip("Maksymalny czas spowolnienia (w sekundach realnego czasu).")]
    public float maxSlowDuration = 5f;

    [Tooltip("Szybkość odnawiania zasobu (sekundy spowolnienia na sekundę realnego czasu).")]
    public float rechargeRate = 1f;

    [Tooltip("Jeśli true, gracz ma nieskończony czas spowolnienia.")]
    public bool infiniteSlow = false;

    [Header("Debug / UI (tylko podgląd)")]
    [Tooltip("Aktualny stan zasobu spowalniania (0..maxSlowDuration).")]
    public float currentSlowTime;

    // Czy aktualnie jesteśmy w trybie slow motion
    private bool isSlowing = false;

    // Domyślne fixedDeltaTime (Unity default to 0.02f)
    private const float defaultFixedDeltaTime = 0.02f;

    // Referencja do PauseController, żeby znać baseTimeScale
    private PauseController pauseController;

    private void Start()
    {
        pauseController = FindFirstObjectByType<PauseController>();
        if (pauseController == null)
        {
            Debug.LogWarning("PlayerTimeSlow: Nie znaleziono PauseController w scenie. " +
                             "Spowalnianie będzie liczone względem Time.timeScale = 1.");
        }

        currentSlowTime = maxSlowDuration;
    }

    private void Update()
    {
        // Jeśli gra jest zapauzowana (ESC) -> nie dotykamy Time.timeScale,
        // ale możemy ładować zasób spowolnienia.
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

        // Shift (lewy lub prawy)
        bool slowKeyHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (slowKeyHeld && (infiniteSlow || currentSlowTime > 0f))
        {
            // Włączone spowalnianie
            if (!isSlowing)
            {
                StartSlow();
            }

            if (!infiniteSlow)
            {
                // Użycie zasobu liczone w realnym czasie (unscaledDeltaTime)
                currentSlowTime = Mathf.Max(
                    0f,
                    currentSlowTime - Time.unscaledDeltaTime
                );

                // Jeśli zasób się skończył w tej klatce
                if (currentSlowTime <= 0f)
                {
                    StopSlow();
                }
            }
        }
        else
        {
            // Klawisz puszczony -> wracamy do normalnego tempa
            if (isSlowing)
            {
                StopSlow();
            }

            // Ładowanie zasobu, gdy nie używamy slow motion
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

        // Spowalniamy względem bazowego tempa, ale nie schodzimy do zera
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

        // Powrót do bazowego tempa gry (np. 0.8)
        Time.timeScale = baseScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
    }
}
