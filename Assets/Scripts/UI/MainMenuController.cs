using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene names")]
    public string gameSceneName = "GameScene";
    public string scoreboardSceneName = "ScoreboardScene"; 
    public string shopSceneName = "ShopScene";

    private void Start()
    {
        // Start menu music after entering scene
        AudioManager.Instance?.PlayMenuMusic();
    }

    // Start gry
    public void OnStartClicked()
    {
        // ======== NOWE ========
        // Start game music before transitioning to scene
        AudioManager.Instance?.PlayGameMusic();

        SceneManager.LoadScene(gameSceneName);
    }

    // Scoreboard
    public void OnScoreboardClicked()
    {
        Debug.Log("Scoreboard not yet implemented.");
    }

    // Sklep
    public void OnShopClicked()
    {
        SceneManager.LoadScene(shopSceneName);
    }

    // Exit game
    public void OnExitClicked()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}