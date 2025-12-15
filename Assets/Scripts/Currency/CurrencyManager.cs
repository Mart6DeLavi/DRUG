using UnityEngine;

/// <summary>
/// Legacy currency manager for backward compatibility.
/// Delegates all operations to PlayerWallet (if available) or GameData.
/// No file I/O in this class - all persistence is handled by GameData.
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    public int Coins 
    { 
        get 
        {
            // Return currency from PlayerWallet if available, otherwise from GameData
            if (PlayerWallet.Instance != null)
            {
                return PlayerWallet.Instance.CurrentCurrency;
            }
            return GameData.GetCurrency();
        }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddCoins(int amount)
    {
        // Delegate to PlayerWallet if available, otherwise use GameData
        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.AddCurrency(amount);
        }
        else
        {
            GameData.AddCurrency(amount);
        }

        CurrencyUI.Instance?.ShowAndFade(Coins);
    }

    public bool SpendCoins(int amount)
    {
        bool success;
        
        // Delegate to PlayerWallet if available, otherwise use GameData
        if (PlayerWallet.Instance != null)
        {
            success = PlayerWallet.Instance.TrySpendCurrency(amount);
        }
        else
        {
            success = GameData.TrySpendCurrency(amount);
        }
        
        if (success)
        {
            CurrencyUI.Instance?.UpdateCoinUI(Coins);
        }
        
        return success;
    }
}
