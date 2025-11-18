using UnityEngine;
using TMPro;

public class ScoreboardDisplay : MonoBehaviour
{
    public TextMeshProUGUI template;
    public Transform listParent;

    void Start()
    {
        var scores = ScoreDatabase.LoadScores();

        foreach (var s in scores)
        {
            var row = Instantiate(template, listParent);
            row.text = $"{s.playerName} — {s.score} pkt — {s.time:F1} s";
        }

        template.gameObject.SetActive(false);
    }
}