using System;
using System.Collections.Generic;

/// <summary>
/// Data structure for storing all player-related data including currency and owned skins.
/// This class is serializable for JSON persistence.
/// </summary>
[Serializable]
public class PlayerData
{
    /// <summary>
    /// Player's current currency amount (coins/money for purchasing items)
    /// </summary>
    public int currency = 0;

    /// <summary>
    /// List of IDs of skins that the player has purchased/unlocked
    /// </summary>
    public List<string> ownedSkinIds = new List<string>();

    /// <summary>
    /// ID of the currently equipped skin (if any)
    /// </summary>
    public string equippedSkinId = "";

    /// <summary>
    /// Timestamp of when data was last saved
    /// </summary>
    public string lastSaveTime = "";

    public PlayerData()
    {
        currency = 0;
        ownedSkinIds = new List<string>();
        equippedSkinId = "";
        lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public PlayerData(int initialCurrency)
    {
        currency = initialCurrency;
        ownedSkinIds = new List<string>();
        equippedSkinId = "";
        lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
