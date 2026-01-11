using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Name of gameplay scene to load after clicking Play Again.")]
    private string gameSceneName = "GameScene";

    // Attach this method to "Play Again" button (OnClick)
    public void OnPlayAgain()
    {
        // Make sure game is not paused
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Clear previous results/round state
        SurvivalScore.ClearLastResults();

        // Load specified scene if available, otherwise reload current
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
