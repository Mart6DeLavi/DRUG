using UnityEngine;
using UnityEngine.SceneManagement; // necessary for changing scenes

public class BackToMenu : MonoBehaviour
{
    /// <summary>
    /// Loads main menu scene and restores normal game time.
    /// Hook this function to a button in UI (OnClick).
    /// </summary>
    public void LoadMainMenu()
    {
        // Just in case, unlock time,
        // because from pause we could enter here with timeScale = 0.
        Time.timeScale = 1f;

        // Make sure the scene "MainMenu" is added in Build Settings.
        SceneManager.LoadScene("MainMenu"); 
    }
}
