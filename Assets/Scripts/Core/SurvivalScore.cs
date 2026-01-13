using UnityEngine;
using TMPro;

public class SurvivalScore : MonoBehaviour
{
    // Points multiplier (buff).
    // IMPORTANT: the multiplier must affect only points earned while the buff is active,
    // not retroactively multiply the entire run at the end.
    private float pointsMultiplier = 1f;
    private float multiplierTimer = 0f;

    // Accumulated score so we can add points per-frame with the current multiplier.
    // (Previous implementation recalculated score from total time, which caused the bug.)
    private float scoreAccumulator = 0f;
    public static SurvivalScore Instance { get; private set; }

    [Header("Konfiguracja punktacji czasu przetrwania")]
    [Tooltip("Ile punktów na sekundę przetrwania.")]
    public float pointsPerSecond = 10f;

    [Header("UI (opcjonalne)")] 
    [Tooltip("Tekst TMP do wyświetlania bieżącego wyniku.")]
    public TextMeshProUGUI scoreText;

    public float TimeSurvived { get; private set; }
    public int CurrentScore { get; private set; }
    public int FinalScore { get; private set; }

    bool isActive = true;

    // Activates (or refreshes) a points multiplier buff.
    public void ActivatePointsMultiplier(float multiplier, float duration)
    {
        if (multiplier <= 0f || duration <= 0f) return;

        // If we picked a stronger multiplier, replace it and reset timer.
        // If it's the same (e.g. another 2x pickup), just refresh/extend the timer.
        if (multiplier > pointsMultiplier)
        {
            pointsMultiplier = multiplier;
            multiplierTimer = duration;
        }
        else
        {
            multiplierTimer = Mathf.Max(multiplierTimer, duration);
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (!isActive) return;

        float dt = Time.deltaTime;
        TimeSurvived += dt;

        // Tick the buff timer first (so it expires cleanly).
        if (multiplierTimer > 0f)
        {
            multiplierTimer -= dt;
            if (multiplierTimer <= 0f)
            {
                multiplierTimer = 0f;
                pointsMultiplier = 1f;
            }
        }

        // Add points for this frame with current multiplier.
        scoreAccumulator += dt * pointsPerSecond * pointsMultiplier;
        CurrentScore = Mathf.FloorToInt(scoreAccumulator);

        if (scoreText != null)
        {
            scoreText.text = $"Wynik: {CurrentScore}";
        }
    }

    public void SealFinalScore()
    {
        if (!isActive) return;
        isActive = false;
        FinalScore = CurrentScore;
        PlayerPrefs.SetInt("LastSurvivalScore", FinalScore);
        PlayerPrefs.SetFloat("LastSurvivalTime", TimeSurvived);
    }

    // Resetuje stan bieżącej rundy (dla nowej gry w tej samej sesji)
    public void ResetState()
    {
        isActive = true;
        TimeSurvived = 0f;
        CurrentScore = 0;
        FinalScore = 0;
        pointsMultiplier = 1f;
        multiplierTimer = 0f;
        scoreAccumulator = 0f;
        if (scoreText != null)
        {
            scoreText.text = "Wynik: 0";
        }
    }

    // Czyści zapamiętany wynik z PlayerPrefs i ewentualny stan instancji
    public static void ClearLastResults()
    {
        PlayerPrefs.DeleteKey("LastSurvivalScore");
        PlayerPrefs.DeleteKey("LastSurvivalTime");
        if (Instance != null)
        {
            Instance.ResetState();
        }
    }

    public static int GetLastScore()
    {
        if (Instance != null && Instance.FinalScore > 0) return Instance.FinalScore;
        return PlayerPrefs.GetInt("LastSurvivalScore", 0);
    }

    public static float GetLastTime()
    {
        if (Instance != null && Instance.FinalScore > 0) return Instance.TimeSurvived;
        return PlayerPrefs.GetFloat("LastSurvivalTime", 0f);
    }
}
