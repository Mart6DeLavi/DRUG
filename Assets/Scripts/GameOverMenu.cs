using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    // Przycisk "Zagraj ponownie"
    public void RetryGame()
    {
        // Za³aduj scenê z gr¹
        SceneManager.LoadScene("SampleScene");
    }

    // Przycisk "WyjdŸ"
    public void QuitGame()
    {
        Debug.Log("Zamykanie gry...");
        Application.Quit();
    }
}
