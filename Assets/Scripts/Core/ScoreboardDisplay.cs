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
        // Wyczyść stare wpisy (poza szablonem)
        foreach (Transform child in listParent)
        {
            if (child != template.transform && child.gameObject != template.gameObject)
            {
                Destroy(child.gameObject);
            }
        }

        // Załaduj wyniki z bazy
        var scores = ScoreDatabase.LoadScores();

        // Wyświetl każdy wynik w formacie: "1. 1200"
        for (int i = 0; i < scores.Count; i++)
        {
            TextMeshProUGUI entry = Instantiate(template, listParent);
            entry.text = $"{i + 1}. {scores[i].score}";
            // Aktywuj AFTER ustawienia tekstu
            entry.enabled = true;
            entry.gameObject.SetActive(true);
        }

        // Ukryj szablon
        template.gameObject.SetActive(false);
    }
}