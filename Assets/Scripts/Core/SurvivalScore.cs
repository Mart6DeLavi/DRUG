using UnityEngine;
using TMPro;

public class SurvivalScore : MonoBehaviour
{
    // Adding a points multiplier feature for buff effects
    private float pointsMultiplier = 1f;
    private float multiplierTimer = 0f;
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

    // Adding Update method to handle multiplier timing
    public void ActivatePointsMultiplier(float multiplier, float duration)
    {
        pointsMultiplier = multiplier;
        multiplierTimer = duration;
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
        TimeSurvived += Time.deltaTime;
        CurrentScore = Mathf.FloorToInt(TimeSurvived * pointsPerSecond);
        if (scoreText != null)
        {
            scoreText.text = $"Wynik: {CurrentScore}";
        }

        // Apply multiplier for BUFF
        CurrentScore = Mathf.FloorToInt(TimeSurvived * pointsPerSecond * pointsMultiplier);

        // Update multiplier timer for BUFF
        if (multiplierTimer > 0f)
        {
            multiplierTimer -= Time.deltaTime;
            if (multiplierTimer <= 0f)
            {
                pointsMultiplier = 1f;
            }
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
