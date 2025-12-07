using System;
using System.Collections.Generic;

/// <summary>
/// Backward compatibility wrapper for GameDatabase.
/// All methods delegate to GameData (the new authoritative API).
/// 
/// DEPRECATED: Use GameData directly for new code.
/// This class exists only to maintain compatibility with existing code.
/// </summary>
[Obsolete("Use GameData instead. GameDatabase is maintained for backward compatibility only.")]
public static class GameDatabase
{
    // Core Load/Save
    public static PlayerData LoadPlayerData() => GameData.LoadPlayerData();
    public static void SavePlayerData(PlayerData data) => GameData.SavePlayerData(data);
    public static void InvalidateCache() => GameData.InvalidateCache();

    // Currency Management (old method names)
    public static int LoadPlayerCurrency() => GameData.GetCurrency();
    public static void SavePlayerCurrency(int amount) => GameData.SetCurrency(amount);
    public static int AddCurrency(int amount) => GameData.AddCurrency(amount);
    public static bool TrySpendCurrency(int amount) => GameData.TrySpendCurrency(amount);

    // Skin Management (old method names)
    public static List<string> LoadOwnedSkins() => GameData.GetOwnedSkins();
    public static bool HasSkin(string skinId) => GameData.HasSkin(skinId);
    public static bool AddOwnedSkin(string skinId) => GameData.AddOwnedSkin(skinId);
    public static bool RemoveOwnedSkin(string skinId) => GameData.RemoveOwnedSkin(skinId);
    public static void SetOwnedSkins(List<string> skinIds)
    {
        // Recreate this behavior using GameData
        PlayerData d = GameData.LoadPlayerData();
        d.ownedSkinIds = new List<string>(skinIds ?? new List<string>());
        GameData.SavePlayerData(d);
    }

    // Equipped Skin (old method names)
    public static string LoadEquippedSkin() => GameData.GetEquippedSkin();
    public static void SaveEquippedSkin(string skinId) => GameData.SetEquippedSkin(skinId);

    // Purchase System
    public static bool PurchaseSkin(string skinId, int price) => GameData.PurchaseSkin(skinId, price);
    public static bool RefundSkin(string skinId, int refundAmount) => GameData.RefundSkin(skinId, refundAmount);

    // Debug/Utility
    public static void DeleteAllPlayerData() => GameData.DeleteAllData();
    public static string GetPlayerDataPath() => GameData.GetPlayerDataPath();
    public static bool SaveFileExists() => GameData.SaveFileExists();
    public static void PrintPlayerData() => GameData.PrintPlayerData();
}
