using UnityEngine;
using TMPro;

public class ScoreboardDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI template;
    [SerializeField] private Transform listParent;

    void Start()
    {
        // Do not call here - will be called from GameOverScoreDisplay after saving
        // ShowResults();
    }

    public void ShowResults()
    {
        // Clear old entries (except template)
        foreach (Transform child in listParent)
        {
            if (child != template.transform && child.gameObject != template.gameObject)
            {
                Destroy(child.gameObject);
            }
        }

        // Load scores from database
        var scores = ScoreDatabase.LoadScores();

        // Display each score in format: "1. 1200"
        for (int i = 0; i < scores.Count; i++)
        {
            TextMeshProUGUI entry = Instantiate(template, listParent);
            entry.text = $"{i + 1}. {scores[i].score}";
            // Activate AFTER setting text
            entry.enabled = true;
            entry.gameObject.SetActive(true);
        }

        // Hide template
        template.gameObject.SetActive(false);
    }
}