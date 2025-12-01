using UnityEngine;
using UnityEngine.SceneManagement; // konieczne do zmiany scen

public class BackToMenu : MonoBehaviour
{
    /// <summary>
    /// Ładuje scenę głównego menu i przywraca normalny czas gry.
    /// Funkcję podpinamy pod przycisk w UI (OnClick).
    /// </summary>
    public void LoadMainMenu()
    {
        // Na wszelki wypadek odblokowujemy czas,
        // bo z pauzy mogliśmy wejść tutaj z timeScale = 0.
        Time.timeScale = 1f;

        // Upewnij się, że scena "MainMenu" jest dodana w Build Settings.
        SceneManager.LoadScene("MainMenu"); 
    }
}
