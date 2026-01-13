using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Name of gameplay scene to reload.")]
    private string gameSceneName = "GameScene";

    // "Play Again" button
    public void RetryGame()
    {
        // Make sure game is not paused
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Clear previous results/round state (if used)
        SurvivalScore.ClearLastResults();

        // Preferred target scene
        string targetScene = string.IsNullOrEmpty(gameSceneName) ? "GameScene" : gameSceneName;

        // If scene exists in Build Settings - load it, otherwise reload current
        if (Application.CanStreamedLevelBeLoaded(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            // Fallback: reload current scene
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
