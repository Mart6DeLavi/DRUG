using System;
using UnityEngine;

/// <summary>
/// Keeps track of the player's soft currency (e.g. points used inside the shop).
/// The value is persisted through PlayerPrefs and exposed via a lightweight singleton.
/// </summary>
public class PlayerWallet : MonoBehaviour
{
    private const string CurrencyKey = "PlayerWallet_Currency";

    public static PlayerWallet Instance { get; private set; }

    [Header("Setup")]
    [Tooltip("Currency that the player starts with if no save exists yet.")]
    [Min(0)]
    public int startingCurrency = 0;

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

        LoadCurrency();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Adds currency (e.g. awarded after a run).
    /// </summary>
    public void AddCurrency(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentCurrency += amount;
        SaveCurrency();
    }

    /// <summary>
    /// Attempts to subtract currency for a purchase.
    /// Returns true if the transaction succeeded.
    /// </summary>
    public bool TrySpendCurrency(int amount)
    {
        if (amount <= 0)
        {
            return true; // Nothing to spend.
        }

        if (CurrentCurrency < amount)
        {
            return false;
        }

        CurrentCurrency -= amount;
        SaveCurrency();
        return true;
    }

    /// <summary>
    /// Overrides the current amount and saves it immediately.
    /// Useful for debug buttons in the editor.
    /// </summary>
    public void SetCurrency(int newValue)
    {
        CurrentCurrency = Mathf.Max(0, newValue);
        SaveCurrency();
    }

    private void LoadCurrency()
    {
        if (PlayerPrefs.HasKey(CurrencyKey))
        {
            CurrentCurrency = Mathf.Max(0, PlayerPrefs.GetInt(CurrencyKey, 0));
        }
        else
        {
            CurrentCurrency = Mathf.Max(0, startingCurrency);
            PlayerPrefs.SetInt(CurrencyKey, CurrentCurrency);
            PlayerPrefs.Save();
        }

        CurrencyChanged?.Invoke(CurrentCurrency);
    }

    private void SaveCurrency()
    {
        PlayerPrefs.SetInt(CurrencyKey, CurrentCurrency);
        PlayerPrefs.Save();
        CurrencyChanged?.Invoke(CurrentCurrency);
    }
}
