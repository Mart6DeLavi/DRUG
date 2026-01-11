using UnityEngine;

public class PauseController : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Pause menu panel (Canvas/Panel)")]
    public GameObject pauseMenuUI;

    [Header("Game Speed")]
    [Tooltip("Default game tempo (1 = normal, 0.8 = slightly slowed).")]
    [Range(0.1f, 2f)]
    public float baseTimeScale = 0.8f;

    private bool isPaused = false;

    // Remember default fixedDeltaTime from project (Unity default 0.02f)
    private const float defaultFixedDeltaTime = 0.02f;

    private void Start()
    {
        // Make sure we don't set 0 or negative value
        if (baseTimeScale <= 0f)
            baseTimeScale = 1f;

        ApplyTimeScale(baseTimeScale);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
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
        if (isPaused) return;

        isPaused = true;
        // Complete time stop
        Time.timeScale = 0f;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        // Return to base tempo (e.g. 0.8 instead of 1.0)
        ApplyTimeScale(baseTimeScale);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    private void ApplyTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * scale;
    }
}
