using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CurrencyUI : MonoBehaviour
{
    public static CurrencyUI Instance;

    public TextMeshProUGUI coinsText;
    public Image coinIcon;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateCoinUI(int coins)
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();
    }
}