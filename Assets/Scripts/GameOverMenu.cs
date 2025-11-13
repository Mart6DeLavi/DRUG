using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    // Przycisk "Zagraj ponownie"
    public void RetryGame()
    {
        // Za�aduj scen� z gr�
        SceneManager.LoadScene("GameScene");
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
