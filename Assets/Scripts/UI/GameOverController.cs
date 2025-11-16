using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Nazwa sceny z rozgrywką, która ma zostać załadowana po kliknięciu Zagraj ponownie.")]
    private string gameSceneName = "GameScene";

    // Przypnij tę metodę do przycisku "Zagraj ponownie" (OnClick)
    public void OnPlayAgain()
    {
        SurvivalScore.ClearLastResults();
        SceneManager.LoadScene(gameSceneName);
    }
}
