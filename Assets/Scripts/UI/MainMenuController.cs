using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Nazwy scen")]
    public string gameSceneName = "GameScene";
    public string scoreboardSceneName = "ScoreboardScene"; 
    public string shopSceneName = "ShopScene";

    private void Start()
    {
        // Uruchamiamy muzykę menu po wejściu na scenę
        AudioManager.Instance?.PlayMenuMusic();
    }

    // Start gry
    public void OnStartClicked()
    {
        // ======== NOWE ========
        // Uruchamiamy muzykę gry przed przejściem do sceny
        AudioManager.Instance?.PlayGameMusic();

        SceneManager.LoadScene(gameSceneName);
    }

    // Scoreboard
    public void OnScoreboardClicked()
    {
        Debug.Log("Scoreboard jeszcze niezaimplementowany.");
    }

    // Sklep
    public void OnShopClicked()
    {
        SceneManager.LoadScene(shopSceneName);
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