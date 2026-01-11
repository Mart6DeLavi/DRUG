using System;
using UnityEngine;

/// <summary>
/// Mirrors player currency from GameData and publishes currency change events.
/// This is NOT the authoritative source - it reads from GameData on initialization
/// and after every operation. Use this for UI updates and event subscriptions.
/// </summary>
public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    [Header("Setup")]
    [Tooltip("Currency that the player starts with if no save file exists yet.")]
    [Min(0)]
    public int startingCurrency = 500;

    [Tooltip("Keep this object alive when changing scenes.")]
    public bool dontDestroyOnLoad = true;

    public int CurrentCurrency { get; private set; }

    /// <summary>
    /// Fired every time the currency value changes.
    /// </summary>
    public event Action<int> CurrencyChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        RefreshFromGameData();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Adds currency via GameData and refreshes local mirror.
    /// </summary>
    public void AddCurrency(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentCurrency = GameData.AddCurrency(amount);
        CurrencyChanged?.Invoke(CurrentCurrency);
    }

    /// <summary>
    /// Attempts to spend currency via GameData. Returns true if successful.
    /// Refreshes local mirror on success.
    /// </summary>
    public bool TrySpendCurrency(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        bool success = GameData.TrySpendCurrency(amount);
        if (success)
        {
            CurrentCurrency = GameData.GetCurrency();
            CurrencyChanged?.Invoke(CurrentCurrency);
        }
        return success;
    }

    /// <summary>
    /// Sets currency to a specific value via GameData and refreshes local mirror.
    /// </summary>
    public void SetCurrency(int newValue)
    {
        GameData.SetCurrency(newValue);
        CurrentCurrency = GameData.GetCurrency();
        CurrencyChanged?.Invoke(CurrentCurrency);
    }

    /// <summary>
    /// Refreshes CurrentCurrency from GameData (the authoritative source).
    /// GameData will handle setting the starting currency (5000) for new players.
    /// </summary>
    public void RefreshFromGameData()
    {
        // Simply get the currency from GameData
        // GameData.LoadPlayerData() will create new data with 5000 starting currency if needed
        CurrentCurrency = GameData.GetCurrency();
        
        Debug.Log($"[PlayerWallet] Currency loaded: {CurrentCurrency}");
        CurrencyChanged?.Invoke(CurrentCurrency);
    }
}
