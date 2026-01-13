using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelOpener : MonoBehaviour
{
    public GameObject Panel;

    public void OpenPanel()
    {
        SceneManager.LoadScene("PanelScene");
    }
}
