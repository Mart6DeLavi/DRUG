using UnityEngine;
using UnityEngine.UI;

public class EffectFrameUI : MonoBehaviour
{
    public static EffectFrameUI Instance;

    public Image frameImage;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (frameImage == null)
            frameImage = GetComponent<Image>();

        gameObject.SetActive(false);
    }

    public void ShowBuffFrame()
    {
        if (frameImage == null) return;
        frameImage.color = new Color(0.4f, 1f, 0.4f, 0.4f); // light green
        gameObject.SetActive(true);
    }

    public void ShowDebuffFrame()
    {
        if (frameImage == null) return;
        frameImage.color = new Color(1f, 0.4f, 0.4f, 0.4f); // light red
        gameObject.SetActive(true);
    }

    public void HideFrame()
    {
        gameObject.SetActive(false);
    }
}
