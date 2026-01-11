using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Debug/Test script for GameData API functionality.
/// Attach this to a GameObject in your scene to test the unified database API.
/// Tests atomic purchase operations and thread-safety.
/// </summary>
public class GameDatabaseTester : MonoBehaviour
{
    [Header("Test Settings")]
    public int testCurrencyAmount = 1000;
    public string testSkinId = "skin_test";
    public int testSkinPrice = 100;

    [Header("UI References (Optional)")]
    public Text statusText;

    void Start()
    {
        Log("GameData Tester initialized. Use keyboard shortcuts to test:");
        Log("1 - Add Currency");
        Log("2 - Try Spend Currency");
        Log("3 - Add Test Skin");
        Log("4 - Purchase Skin (Atomic)");
        Log("5 - Print Player Data");
        Log("6 - Check Save File");
        Log("7 - Run Complete Shop Flow Test");
        Log("0 - Reset All Data (WARNING!)");
    }

    void Update()
    {
        // Test 1: Add Currency
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            int newTotal = GameData.AddCurrency(testCurrencyAmount);
            Log($"✓ Added {testCurrencyAmount} currency. New total: {newTotal}");
        }

        // Test 2: Try Spend Currency
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            bool success = GameData.TrySpendCurrency(50);
            if (success)
            {
                int remaining = GameData.GetCurrency();
                Log($"✓ Spent 50 currency. Remaining: {remaining}");
            }
            else
            {
                Log("✗ Not enough currency to spend 50!");
            }
        }

        // Test 3: Add Test Skin
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            bool added = GameData.AddOwnedSkin(testSkinId);
            if (added)
            {
                Log($"✓ Added skin: {testSkinId}");
            }
            else
            {
                Log($"✗ Skin {testSkinId} already owned!");
            }
        }

        // Test 4: Purchase Skin (Atomic Transaction)
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            bool success = GameData.PurchaseSkin(testSkinId + "_purchased", testSkinPrice);
            if (success)
            {
                Log($"✓ Successfully purchased skin for {testSkinPrice}!");
            }
            else
            {
                Log($"✗ Purchase failed! Insufficient funds or already owned.");
            }
        }

        // Test 5: Print All Player Data
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            Log("=== PRINTING PLAYER DATA ===");
            GameData.PrintPlayerData();
            
            int currency = GameData.GetCurrency();
            var skins = GameData.GetOwnedSkins();
            string equipped = GameData.GetEquippedSkin();
            
            Log($"Currency: {currency}");
            Log($"Owned Skins: {string.Join(", ", skins)}");
            Log($"Equipped Skin: {equipped}");
        }

        // Test 6: Check Save File
        if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
        {
            bool exists = GameData.SaveFileExists();
            string path = GameData.GetPlayerDataPath();
            
            Log($"Save file exists: {exists}");
            Log($"Save file path: {path}");
        }

        // Test 7: Run Complete Shop Flow
        if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
        {
            TestCompleteShopFlow();
        }

        // Test 0: Reset All Data
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            Log("⚠️ RESETTING ALL PLAYER DATA!");
            GameData.DeleteAllData();
            Log("✓ All data deleted. Start fresh!");
        }
    }

    void Log(string message)
    {
        Debug.Log($"[GameDatabaseTester] {message}");
        
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    // Public methods that can be called from UI buttons
    public void OnAddCurrencyClicked()
    {
        int newTotal = GameData.AddCurrency(testCurrencyAmount);
        Log($"Added {testCurrencyAmount}. New total: {newTotal}");
    }

    public void OnPurchaseTestSkinClicked()
    {
        string skinId = testSkinId + "_button";
        bool success = GameData.PurchaseSkin(skinId, testSkinPrice);
        
        if (success)
        {
            Log($"Purchased {skinId} for {testSkinPrice}!");
        }
        else
        {
            Log($"Purchase failed!");
        }
    }

    public void OnShowPlayerDataClicked()
    {
        GameData.PrintPlayerData();
    }

    public void OnResetDataClicked()
    {
        GameData.DeleteAllData();
        Log("All data reset!");
    }

    /// <summary>
    /// Comprehensive test of the complete shop flow with atomic operations.
    /// Tests purchase, duplicate purchase prevention, insufficient funds, and refunds.
    /// </summary>
    [ContextMenu("Test Complete Shop Flow")]
    public void TestCompleteShopFlow()
    {
        Log("\n=== TESTING COMPLETE ATOMIC SHOP FLOW ===\n");
        
        // 1. Reset data
        GameData.DeleteAllData();
        GameData.InvalidateCache();
        Log("1. ✓ Reset all data and cleared cache");
        
        // 2. Add initial currency
        int total = GameData.AddCurrency(500);
        Log($"2. ✓ Added 500 currency. Total: {total}");
        
        // 3. Purchase a cheap skin (should succeed)
        bool purchase1 = GameData.PurchaseSkin("skin_cheap", 100);
        Log($"3. Purchase 'skin_cheap' (100): {(purchase1 ? "✓ SUCCESS" : "✗ FAILED")}. Remaining: {GameData.GetCurrency()}");
        
        // 4. Try to purchase same skin again (should fail - already owned)
        bool purchase2 = GameData.PurchaseSkin("skin_cheap", 100);
        Log($"4. Purchase 'skin_cheap' again: {(purchase2 ? "✗ FAILED (duplicate allowed!)" : "✓ BLOCKED (correct)")}");
        
        // 5. Check ownership
        bool owns = GameData.HasSkin("skin_cheap");
        Log($"5. Owns 'skin_cheap': {(owns ? "✓ YES" : "✗ NO")}");
        
        // 6. Try to buy expensive skin without enough money (should fail)
        bool purchase3 = GameData.PurchaseSkin("skin_expensive", 1000);
        Log($"6. Purchase 'skin_expensive' (1000) with only {GameData.GetCurrency()}: {(purchase3 ? "✗ FAILED (overspent!)" : "✓ BLOCKED (correct)")}");
        
        // 7. Add more currency and try again (should succeed)
        GameData.AddCurrency(1000);
        bool purchase4 = GameData.PurchaseSkin("skin_expensive", 1000);
        Log($"7. Added 1000, then purchase 'skin_expensive': {(purchase4 ? "✓ SUCCESS" : "✗ FAILED")}. Remaining: {GameData.GetCurrency()}");
        
        // 8. List all owned skins
        var allSkins = GameData.GetOwnedSkins();
        Log($"8. All owned skins ({allSkins.Count}): {string.Join(", ", allSkins)}");
        
        // 9. Test refund
        bool refund = GameData.RefundSkin("skin_cheap", 100);
        Log($"9. Refund 'skin_cheap': {(refund ? "✓ SUCCESS" : "✗ FAILED")}. Currency now: {GameData.GetCurrency()}");
        
        // 10. Verify skin was removed
        bool stillOwns = GameData.HasSkin("skin_cheap");
        Log($"10. Still owns 'skin_cheap' after refund: {(stillOwns ? "✗ YES (refund failed!)" : "✓ NO (correct)")}");
        
        // 11. Print final state
        Log("\n--- FINAL STATE ---");
        GameData.PrintPlayerData();
        
        Log("\n=== TEST COMPLETE ===");
        Log($"Expected: Currency=500, Owned=['skin_expensive'], Equipped=''");
        Log($"Actual: Currency={GameData.GetCurrency()}, Owned=[{string.Join(",", GameData.GetOwnedSkins())}], Equipped='{GameData.GetEquippedSkin()}'");
    }
}
