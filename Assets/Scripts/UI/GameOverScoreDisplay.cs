using UnityEngine;
using TMPro;

public class GameOverScoreDisplay : MonoBehaviour
{
    [Tooltip("TMP text to display final score.")]
    public TextMeshProUGUI finalScoreText;

    [Tooltip("TMP text to display survival time.")]
    public TextMeshProUGUI timeText;

    void Start()
    {
        // Get last score from SurvivalScore
        int score = SurvivalScore.GetLastScore();
        float time = SurvivalScore.GetLastTime();

        // Display on Game Over screen
        if (finalScoreText != null)
        {
            finalScoreText.text = $"{score}";
        }

        if (timeText != null)
        {
            timeText.text = $"Czas przetrwania: {time:F1} s";
        }

        // ===== SAVE TO SCORE DATABASE =====

        // Save only if score > 0
        if (score > 0)
        {
            // Date in text format (easy to save in JSON)
            string date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            // Create record and add it to database
            ScoreRecord record = new ScoreRecord(
                "PLAYER",   // you can substitute player nickname later
                score,
                time,
                date
            );

            ScoreDatabase.AddScore(record);

            // Refresh scoreboard display if it exists
            ScoreboardDisplay scoreboard = FindObjectOfType<ScoreboardDisplay>();
            if (scoreboard != null)
            {
                scoreboard.ShowResults();
            }
        }

        // Clear saved score so it won't be added again on next run
        SurvivalScore.ClearLastResults();
    }
}