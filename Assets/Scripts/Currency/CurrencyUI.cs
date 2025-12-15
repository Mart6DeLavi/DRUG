using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CurrencyUI : MonoBehaviour
{
    public static CurrencyUI Instance;

    public TextMeshProUGUI coinsText;
    public Image coinIcon;
    public CanvasGroup canvasGroup;

    public float visibleTime = 1.5f;  // how long to stay visible
    public float fadeDuration = 0.8f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        // Start with the UI hidden
        if (CurrencyManager.Instance != null)
            UpdateCoinUI(CurrencyManager.Instance.Coins);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    public void UpdateCoinUI(int coins)
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();
    }

    public void ShowAndFade(int coins)
    {
        UpdateCoinUI(coins);
        if (canvasGroup == null) return;

        StopAllCoroutines();
        StartCoroutine(FadeRoutine());
    }

    System.Collections.IEnumerator FadeRoutine()
    {
        canvasGroup.alpha = 1f;                     // show immediately
        yield return new WaitForSeconds(visibleTime);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}