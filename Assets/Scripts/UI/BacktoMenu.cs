using UnityEngine;
using UnityEngine.SceneManagement; // konieczne do zmiany scen

public class BackToMenu : MonoBehaviour
{
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // nazwa sceny do za³adowania
    }
}
