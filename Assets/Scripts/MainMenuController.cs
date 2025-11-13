using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Nazwy scen")]
    public string gameSceneName = "GameScene";
    public string scoreboardSceneName = "ScoreboardScene"; // na razie może nie istnieć

    // Start gry
    public void OnStartClicked()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Scoreboard
    public void OnScoreboardClicked()
    {
        // Jeśli jeszcze nie masz sceny Scoreboard, na razie tylko log:
        Debug.Log("Scoreboard jeszcze niezaimplementowany.");
        // Jak zrobimy scenę:
        // SceneManager.LoadScene(scoreboardSceneName);
    }

    // Wyjście z gry
    public void OnExitClicked()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}