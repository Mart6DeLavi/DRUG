using UnityEngine;

/// <summary>
/// Helper component to reset player data from Unity Inspector.
/// Add this to any GameObject in your scene.
/// Buttons for Reset, Print, and Force Load will appear in Inspector.
/// </summary>
public class GameDataResetter : MonoBehaviour
{
    // Сброс сохранений
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

    // Вывод текущих данных сохранения
    public void PrintSaveData()
    {
        GameData.PrintPlayerData();
    }

    // Загрузка свежего сохранения
    public void ForceLoadFresh()
    {
        GameData.ResetPlayerData();
        PlayerData data = GameData.LoadPlayerData();
        Debug.Log($"[GameDataResetter] Fresh save loaded: Currency={data.currency}, Skins={data.ownedSkinIds.Count}");
    }
}
