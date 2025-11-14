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
        int score = SurvivalScore.GetLastScore();
        float time = SurvivalScore.GetLastTime();

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Ostateczny wynik: {score}";
        }
        if (timeText != null)
        {
            timeText.text = $"Czas przetrwania: {time:F1} s";
        }
    }
}
