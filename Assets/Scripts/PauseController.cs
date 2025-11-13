using UnityEngine;

public class PauseController : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Panel z menu pauzy (Canvas/Panel)")]
    public GameObject pauseMenuUI;

    private bool isPaused = false;

    private void Start()
    {
        ResumeGame(); // upewniamy się że timescale = 1
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }
}