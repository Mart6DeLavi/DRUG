using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;

    public void OpenTutorial()
    {
        tutorialPanel.SetActive(true);
    }

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
    }
}
