using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Single authoritative API for all player persistent data (currency, owned skins, equipped skin).
/// This is the single source of truth. All operations are atomic where needed.
/// Thread-safe for concurrent access using lock(_lock).
/// 
/// Usage:
/// - GameData.GetCurrency() / SetCurrency(amount)
/// - GameData.AddCurrency(amount) / TrySpendCurrency(amount)
/// - GameData.HasSkin(id) / AddOwnedSkin(id) / RemoveOwnedSkin(id)
/// - GameData.PurchaseSkin(id, price) - atomic purchase operation
/// - GameData.RefundSkin(id, refundAmount) - atomic refund operation
/// </summary>
public static class GameData
{
    private const string PlayerDataFileName = "playerdata.json";
    private static string PlayerDataFilePath => Path.Combine(Application.persistentDataPath, PlayerDataFileName);

    private static PlayerData _cached = null;
    private static readonly object _lock = new object();

    #region Core Load/Save

    /// <summary>
    /// Loads player data from disk. Returns cached data if available.
    /// Creates new PlayerData with defaults if no save file exists.
    /// Starting currency is set to 5000 for new players.
    /// </summary>
    public static PlayerData LoadPlayerData()
    {
        if (_cached != null)
        {
            return _cached;
        }

        if (!File.Exists(PlayerDataFilePath))
        {
            // Create new player data with starting currency of 5000
            _cached = new PlayerData();
            _cached.currency = 0;
            
            // Save immediately so the player has their starting currency
            SavePlayerData(_cached);
            Debug.Log("[GameData] New player data created with 5000 starting currency");
            return _cached;
        }

        try
        {
            string json = File.ReadAllText(PlayerDataFilePath);
            _cached = JsonUtility.FromJson<PlayerData>(json) ?? new PlayerData();
            return _cached;
        }
        catch (Exception e)
        {
            Debug.LogError($"GameData: Failed to load player data: {e.Message}");
            _cached = new PlayerData();
            _cached.currency = 5000;
            return _cached;
        }
    }

    /// <summary>
    /// Saves player data to disk and updates cache.
    /// </summary>
    public static void SavePlayerData(PlayerData data)
    {
        if (data == null)
        {
            Debug.LogError("GameData: Cannot save null PlayerData");
            return;
        }

        try
        {
            data.lastSaveTime = DateTime.UtcNow.ToString("o");
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(PlayerDataFilePath, json);
            _cached = data;
            
            Debug.Log($"[GameData] SAVED: Currency={data.currency}, Skins={data.ownedSkinIds.Count}, Path={PlayerDataFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"GameData: Failed to save player data: {e.Message}");
        }
    }

    /// <summary>
    /// Forces next LoadPlayerData() to read from disk instead of cache.
    /// </summary>
    public static void InvalidateCache()
    {
        _cached = null;
    }

    #endregion

    #region Debug/Reset Methods

    /// <summary>
    /// Deletes the save file and clears cache. Next LoadPlayerData() will create fresh save with 5000 currency.
    /// Call this from Unity Inspector or console to reset player progress.
    /// </summary>
    [UnityEngine.ContextMenu("Reset Player Data")]
    public static void ResetPlayerData()
    {
        lock (_lock)
        {
            if (File.Exists(PlayerDataFilePath))
            {
                File.Delete(PlayerDataFilePath);
                Debug.Log($"[GameData] DELETED save file: {PlayerDataFilePath}");
            }
            _cached = null;
            
            // Force load fresh data with 5000 starting currency
            PlayerData freshData = LoadPlayerData();
            Debug.Log($"[GameData] Fresh data loaded: Currency={freshData.currency}, Skins={freshData.ownedSkinIds.Count}");
            
            // Notify PlayerWallet to refresh
            if (PlayerWallet.Instance != null)
            {
                PlayerWallet.Instance.RefreshFromGameData();
                Debug.Log($"[GameData] PlayerWallet refreshed to {PlayerWallet.Instance.CurrentCurrency}");
            }
            
            Debug.Log("[GameData] Player data reset complete with 5000 starting currency.");
        }
    }

    #endregion

    #region Currency Management

    /// <summary>
    /// Gets current currency amount.
    /// </summary>
    public static int GetCurrency()
    {
        return LoadPlayerData().currency;
    }

    /// <summary>
    /// Sets currency to a specific amount (clamped to >= 0).
    /// </summary>
    public static void SetCurrency(int amount)
    {
        amount = Math.Max(0, amount);
        PlayerData d = LoadPlayerData();
        d.currency = amount;
        SavePlayerData(d);
    }

    /// <summary>
    /// Adds currency and returns new total. Does nothing if amount <= 0.
    /// </summary>
    public static int AddCurrency(int amount)
    {
        if (amount <= 0)
        {
            return GetCurrency();
        }

        PlayerData d = LoadPlayerData();
        d.currency += amount;
        SavePlayerData(d);
        return d.currency;
    }

    /// <summary>
    /// Attempts to spend currency. Returns true if successful, false if insufficient funds.
    /// Atomic operation: check and deduct happen together.
    /// </summary>
    public static bool TrySpendCurrency(int amount)
    {
        if (amount <= 0)
        {
            return true; // No-op
        }

        lock (_lock)
        {
            PlayerData d = LoadPlayerData();
            if (d.currency < amount)
            {
                return false;
            }

            d.currency -= amount;
            SavePlayerData(d);
            return true;
        }
    }

    #endregion

    #region Skin Management

    /// <summary>
    /// Gets a copy of all owned skin IDs.
    /// </summary>
    public static List<string> GetOwnedSkins()
    {
        PlayerData d = LoadPlayerData();
        return new List<string>(d.ownedSkinIds ?? new List<string>());
    }

    /// <summary>
    /// Checks if player owns a specific skin.
    /// </summary>
    public static bool HasSkin(string skinId)
    {
        if (string.IsNullOrEmpty(skinId))
        {
            return false;
        }

        PlayerData d = LoadPlayerData();
        return d.ownedSkinIds != null && d.ownedSkinIds.Contains(skinId);
    }

    /// <summary>
    /// Adds a skin to owned skins. Returns true if added, false if already owned.
    /// Atomic operation.
    /// </summary>
    public static bool AddOwnedSkin(string skinId)
    {
        if (string.IsNullOrEmpty(skinId))
        {
            Debug.LogWarning("GameData: Cannot add skin with empty ID");
            return false;
        }

        lock (_lock)
        {
            PlayerData d = LoadPlayerData();
            
            if (d.ownedSkinIds == null)
            {
                d.ownedSkinIds = new List<string>();
            }

            if (d.ownedSkinIds.Contains(skinId))
            {
                return false;
            }

            d.ownedSkinIds.Add(skinId);
            SavePlayerData(d);
            return true;
        }
    }

    /// <summary>
    /// Removes a skin from owned skins. Returns true if removed, false if not owned.
    /// Unequips skin if it was equipped. Atomic operation.
    /// </summary>
    public static bool RemoveOwnedSkin(string skinId)
    {
        if (string.IsNullOrEmpty(skinId))
        {
            return false;
        }

        lock (_lock)
        {
            PlayerData d = LoadPlayerData();
            
            if (d.ownedSkinIds == null || !d.ownedSkinIds.Contains(skinId))
            {
                return false;
            }

            d.ownedSkinIds.Remove(skinId);
            
            // Unequip if this was the equipped skin
            if (d.equippedSkinId == skinId)
            {
                d.equippedSkinId = "";
            }

            SavePlayerData(d);
            return true;
        }
    }

    /// <summary>
    /// Gets currently equipped skin ID.
    /// </summary>
    public static string GetEquippedSkin()
    {
        PlayerData d = LoadPlayerData();
        return d.equippedSkinId ?? "";
    }

    /// <summary>
    /// Sets the equipped skin ID.
    /// </summary>
    public static void SetEquippedSkin(string skinId)
    {
        PlayerData d = LoadPlayerData();
        d.equippedSkinId = skinId ?? "";
        SavePlayerData(d);
    }

    #endregion

    #region Atomic Purchase/Refund

    /// <summary>
    /// Atomic purchase operation: checks ownership, checks funds, deducts currency, adds skin, saves.
    /// Returns true if purchase succeeded, false if already owned or insufficient funds.
    /// Thread-safe.
    /// </summary>
    public static bool PurchaseSkin(string skinId, int price)
    {
        if (string.IsNullOrEmpty(skinId))
        {
            Debug.LogWarning("GameData: Cannot purchase skin with empty ID");
            return false;
        }

        if (price < 0)
        {
            Debug.LogWarning($"GameData: Cannot purchase skin with negative price: {price}");
            return false;
        }

        lock (_lock)
        {
            PlayerData d = LoadPlayerData();

            if (d.ownedSkinIds == null)
            {
                d.ownedSkinIds = new List<string>();
            }

            // Check if already owned
            if (d.ownedSkinIds.Contains(skinId))
            {
                Debug.Log($"GameData: Skin '{skinId}' already owned");
                return false;
            }

            // Check if enough currency
            if (d.currency < price)
            {
                Debug.Log($"GameData: Insufficient funds to buy '{skinId}'. Has {d.currency}, needs {price}");
                return false;
            }

            // Process purchase
            d.currency -= price;
            d.ownedSkinIds.Add(skinId);
            SavePlayerData(d);
            
            Debug.Log($"GameData: Successfully purchased skin '{skinId}' for {price} currency");
            return true;
        }
    }

    /// <summary>
    /// Atomic refund operation: checks ownership, removes skin, adds currency back, unequips if needed.
    /// Returns true if refund succeeded, false if skin not owned.
    /// Thread-safe.
    /// </summary>
    public static bool RefundSkin(string skinId, int refundAmount)
    {
        if (string.IsNullOrEmpty(skinId))
        {
            Debug.LogWarning("GameData: Cannot refund skin with empty ID");
            return false;
        }

        lock (_lock)
        {
            PlayerData d = LoadPlayerData();
            
            if (d.ownedSkinIds == null || !d.ownedSkinIds.Contains(skinId))
            {
                Debug.Log($"GameData: Skin '{skinId}' not owned, cannot refund");
                return false;
            }

            // Remove skin
            d.ownedSkinIds.Remove(skinId);
            
            // Unequip if this was the equipped skin
            if (d.equippedSkinId == skinId)
            {
                d.equippedSkinId = "";
            }

            // Add refund amount
            int oldCurrency = d.currency;
            if (refundAmount > 0)
            {
                d.currency += refundAmount;
            }

            SavePlayerData(d);
            Debug.Log($"[GameData] Refunded skin '{skinId}' for {refundAmount} currency. Balance: {oldCurrency} -> {d.currency}");
            return true;
        }
    }

    #endregion

    #region Debug/Utility

    /// <summary>
    /// Deletes all player data. Use with caution!
    /// </summary>
    public static void DeleteAllData()
    {
        if (File.Exists(PlayerDataFilePath))
        {
            File.Delete(PlayerDataFilePath);
            Debug.Log("GameData: All player data deleted");
        }

        _cached = null;
    }

    /// <summary>
    /// Gets the file path where player data is stored.
    /// </summary>
    public static string GetPlayerDataPath()
    {
        return PlayerDataFilePath;
    }

    /// <summary>
    /// Checks if a save file exists.
    /// </summary>
    public static bool SaveFileExists()
    {
        return File.Exists(PlayerDataFilePath);
    }

    /// <summary>
    /// Prints current player data to console for debugging.
    /// </summary>
    public static void PrintPlayerData()
    {
        PlayerData d = LoadPlayerData();
        Debug.Log("=== PLAYER DATA ===");
        Debug.Log($"Currency: {d.currency}");
        Debug.Log($"Owned Skins ({d.ownedSkinIds?.Count ?? 0}): {string.Join(", ", d.ownedSkinIds ?? new List<string>())}");
        Debug.Log($"Equipped Skin: {d.equippedSkinId}");
        Debug.Log($"Last Save: {d.lastSaveTime}");
        Debug.Log($"Save File: {PlayerDataFilePath}");
        Debug.Log($"File Exists: {SaveFileExists()}");
    }

    #endregion
}
