using UnityEngine;
using UnityEngine.UI;

public class ButtonClickSound : MonoBehaviour
{
    private void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayButtonClick();
        });
    }
}