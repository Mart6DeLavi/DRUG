using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Nazwa sceny z rozgrywką do ponownego wczytania.")]
    private string gameSceneName = "GameScene";

    // Przycisk "Zagraj ponownie"
    public void RetryGame()
    {
        // Upewnij się, że gra nie jest spauzowana
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Wyczyść poprzednie wyniki/stan rundy (jeśli używany)
        SurvivalScore.ClearLastResults();

        // Preferowana docelowa scena
        string targetScene = string.IsNullOrEmpty(gameSceneName) ? "GameScene" : gameSceneName;

        // Jeśli scena istnieje w Build Settings – wczytaj ją, w przeciwnym razie przeładuj bieżącą
        if (Application.CanStreamedLevelBeLoaded(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            // Fallback: przeładuj aktualną scenę
            Scene active = SceneManager.GetActiveScene();
            SceneManager.LoadScene(active.buildIndex);
        }
    }

    // Przycisk "Wyjd�"
    public void QuitGame()
    {
        Debug.Log("Zamykanie gry...");
        Application.Quit();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
