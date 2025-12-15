using UnityEngine;
using TMPro;

public class GameOverScoreDisplay : MonoBehaviour
{
    [Tooltip("Tekst TMP do wyświetlenia końcowego wyniku.")]
    public TextMeshProUGUI finalScoreText;

    [Tooltip("Tekst TMP do wyświetlenia czasu przetrwania.")]
    public TextMeshProUGUI timeText;

    void Start()
    {
        // Pobieramy ostatni wynik z SurvivalScore
        int score = SurvivalScore.GetLastScore();
        float time = SurvivalScore.GetLastTime();

        // Wyświetlenie na ekranie Game Over
        if (finalScoreText != null)
        {
            finalScoreText.text = $"{score}";
        }

        if (timeText != null)
        {
            timeText.text = $"Czas przetrwania: {time:F1} s";
        }

        // ===== ZAPIS DO BAZY WYNIKÓW =====

        // Zapisujemy tylko jeśli wynik > 0
        if (score > 0)
        {
            // Data w formie tekstu (łatwa do zapisania w JSON)
            string date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            // Tworzymy rekord i dodajemy go do bazy
            ScoreRecord record = new ScoreRecord(
                "PLAYER",   // tu później możesz podstawić nick gracza
                score,
                time,
                date
            );

            ScoreDatabase.AddScore(record);

            // Odświeżamy wyświetlacz tablicy wyników, jeśli istnieje
            ScoreboardDisplay scoreboard = Object.FindFirstObjectByType<ScoreboardDisplay>();
            if (scoreboard != null)
            {
                scoreboard.ShowResults();
            }
        }

        // Czyścimy zapisany wynik, żeby nie dodać go ponownie przy następnym uruchomieniu
        SurvivalScore.ClearLastResults();
    }
}