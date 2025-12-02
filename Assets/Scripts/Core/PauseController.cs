using UnityEngine;

public class PauseController : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Panel z menu pauzy (Canvas/Panel)")]
    public GameObject pauseMenuUI;

    [Header("Game Speed")]
    [Tooltip("Domyślne tempo gry (1 = normalnie, 0.8 = lekko zwolnione).")]
    [Range(0.1f, 2f)]
    public float baseTimeScale = 0.8f;

    private bool isPaused = false;

    // Zapamiętujemy domyślne fixedDeltaTime z projektu (Unity domyślnie 0.02f)
    private const float defaultFixedDeltaTime = 0.02f;

    private void Start()
    {
        // Upewniamy się, że nie ustawimy 0 lub wartości ujemnej
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
        // Całkowite zatrzymanie czasu
        Time.timeScale = 0f;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        // Powrót do bazowego tempa (np. 0.8 zamiast 1.0)
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
