using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Nazwa sceny z rozgrywką, która ma zostać załadowana po kliknięciu Zagraj ponownie.")]
    private string gameSceneName = "GameScene";

    // Przypnij tę metodę do przycisku "Zagraj ponownie" (OnClick)
    public void OnPlayAgain()
    {
        // Upewnij się, że gra nie jest spauzowana
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Wyczyść poprzednie wyniki/stan rundy
        SurvivalScore.ClearLastResults();

        // Wczytaj wskazaną scenę jeśli dostępna, inaczej przeładuj bieżącą
        string targetScene = string.IsNullOrEmpty(gameSceneName) ? "GameScene" : gameSceneName;
        if (Application.CanStreamedLevelBeLoaded(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Scene active = SceneManager.GetActiveScene();
            SceneManager.LoadScene(active.buildIndex);
        }
    }
}
