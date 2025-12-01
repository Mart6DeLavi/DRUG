using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    public int Coins { get; private set; } = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        CurrencyUI.Instance?.UpdateCoinUI(Coins);
    }

    public bool SpendCoins(int amount)
    {
        if (Coins >= amount)
        {
            Coins -= amount;
            CurrencyUI.Instance?.UpdateCoinUI(Coins);
            return true;
        }
        return false;
    }
}
