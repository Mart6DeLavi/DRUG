using UnityEngine;

/// <summary>
/// Helper component to reset player data from Unity Inspector.
/// Add this to any GameObject in your scene.
/// Buttons for Reset, Print, and Force Load will appear in Inspector.
/// </summary>
public class GameDataResetter : MonoBehaviour
{
    // Reset save data
    public void ResetPlayerData()
    {
        GameData.ResetPlayerData();
        Debug.Log("[GameDataResetter] Save reset complete! Currency should now show 5000.");
        
        // If in ShopScene, refresh the UI
        #if UNITY_2023_1_OR_NEWER
        SkinShopController shop = FindFirstObjectByType<SkinShopController>();
        #else
        SkinShopController shop = FindObjectOfType<SkinShopController>();
        #endif
        
        if (shop != null)
        {
            shop.RefreshUI();
            Debug.Log("[GameDataResetter] Shop UI refreshed.");
        }
    }

    // Output current save data
    public void PrintSaveData()
    {
        GameData.PrintPlayerData();
    }

    // Load fresh save
    public void ForceLoadFresh()
    {
        GameData.ResetPlayerData();
        PlayerData data = GameData.LoadPlayerData();
        Debug.Log($"[GameDataResetter] Fresh save loaded: Currency={data.currency}, Skins={data.ownedSkinIds.Count}");
    }
    
    // Show current player data (call from Inspector)
    [ContextMenu("Debug: Show Player Data")]
    public void DebugShowPlayerData()
    {
        PlayerData data = GameData.LoadPlayerData();
        Debug.Log($"[DEBUG] === PLAYER DATA ===");
        Debug.Log($"[DEBUG] Currency: {data.currency}");
        Debug.Log($"[DEBUG] Owned Skins Count: {data.ownedSkinIds?.Count ?? 0}");
        if (data.ownedSkinIds != null && data.ownedSkinIds.Count > 0)
        {
            Debug.Log($"[DEBUG] Owned Skin IDs: {string.Join(", ", data.ownedSkinIds)}");
        }
        else
        {
            Debug.Log($"[DEBUG] No skins owned");
        }
        Debug.Log($"[DEBUG] Equipped Skin: '{data.equippedSkinId}'");
        Debug.Log($"[DEBUG] ==================");
    }
}
