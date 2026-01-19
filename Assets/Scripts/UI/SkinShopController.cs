using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// Handles the UI logic for the skin shop: browsing, buying, previewing and equipping skins.
/// Uses GameDatabase API for persisting purchased skins and currency.
/// </summary>
public class SkinShopController : MonoBehaviour
{

    [Serializable]
    public class SkinDefinition
    {
        [Tooltip("Unique identifier used for saves. If empty the display name will be used instead.")]
        public string id;

        [Tooltip("Name displayed in the UI.")]
        public string displayName;

        [Tooltip("Sprite shown inside the carousel.")]
        public Sprite previewSprite;

        [Tooltip("Price in soft currency.")]
        [Min(0)] public int price = 100;

        [Tooltip("If enabled the skin is owned from the beginning.")]
        public bool unlockedByDefault = false;

        [Header("Animator")]
        [Tooltip("Animator used by this skin.")]
        public RuntimeAnimatorController animatorController;
    }

    [Header("Catalog")]
    public List<SkinDefinition> skins = new List<SkinDefinition>();

    [Header("Preview images (left/center/right)")]
    public Image leftPreview;
    public Image centerPreview;
    public Image rightPreview;

    [Header("Labels")]
    public TextMeshProUGUI skinNameLabel;
    public TextMeshProUGUI priceLabel;
    public TextMeshProUGUI currencyLabel;

    [Header("Buttons")]
    public Button previousButton;
    public Button nextButton;
    public Button buyButton;
    public TextMeshProUGUI buyButtonLabel;
    public Button previewButton;
    public Button refundButton;
    public TextMeshProUGUI refundButtonLabel;

    [Header("Events")]
    public UnityEvent<SkinDefinition> onSkinPreview;
    public UnityEvent<SkinDefinition> onSkinRefunded;

    private readonly HashSet<string> purchasedSkinIds = new HashSet<string>();
    private int currentIndex;

    void OnEnable()
    {
        Debug.Log($"[SHOP] SkinShopController OnEnable - Starting initialization");
        Debug.Log($"[SHOP] Current GameData currency: {GameData.GetCurrency()}");
        
        SubscribeToWallet();
        LoadState();
        RefreshUI();

        
        Debug.Log($"[SHOP] Initialization complete. Skins loaded: {skins?.Count ?? 0}");
    }

    void OnDisable()
    {
        UnsubscribeFromWallet();
    }

    // --- UI HOOKS (set these on the buttons) ---
    public void ShowNextSkin()
    {
        if (skins.Count == 0)
        {
            Debug.Log("SkinShopController: ShowNextSkin ignored because catalog is empty.");
            return;
        }

        currentIndex = WrapIndex(currentIndex + 1);
        Debug.Log($"SkinShopController: Next pressed -> index {currentIndex}/{skins.Count - 1}");
        RefreshUI();
    }

    public void ShowPreviousSkin()
    {
        if (skins.Count == 0)
        {
            Debug.Log("SkinShopController: ShowPreviousSkin ignored because catalog is empty.");
            return;
        }

        currentIndex = WrapIndex(currentIndex - 1);
        Debug.Log($"SkinShopController: Previous pressed -> index {currentIndex}/{skins.Count - 1}");
        RefreshUI();
    }

    public void BuyCurrentSkin()
    {
        Debug.Log("[SHOP] Buy button clicked!");
        
        if (!TryGetCurrentSkin(out SkinDefinition skin))
        {
            Debug.LogWarning("[SHOP] No current skin to buy");
            return;
        }
        
        if (IsSkinOwned(skin))
        {
            Debug.Log("[SHOP] Skin already owned");
            return;
        }

        string skinId = GetSkinId(skin);
        int price = skin.price;
        int currentCurrency = GameData.GetCurrency();
        
        Debug.Log($"[SHOP] Attempting to purchase: {skinId} for {price} (Current: {currentCurrency})");

        // Atomic purchase via GameData
        bool success = GameData.PurchaseSkin(skinId, price);
        if (!success)
        {
            Debug.Log($"[SHOP] Purchase FAILED: {skinId}. Insufficient funds or already owned.");
            return;
        }

        Debug.Log($"[SHOP] Purchase SUCCESS! New balance: {GameData.GetCurrency()}");

        // Update local cache & UI
        purchasedSkinIds.Add(skinId);
        RefreshUI();

        // Ensure PlayerWallet reflects new value
        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.SetCurrency(GameData.GetCurrency());
        }
    }

    public void PreviewCurrentSkin()
    {
        if (!TryGetCurrentSkin(out SkinDefinition skin)) return;
        onSkinPreview?.Invoke(skin);
    }

    public void RefundCurrentSkin()
    {
        Debug.Log("[SHOP] Ubierz button clicked!");
        
        if (!TryGetCurrentSkin(out SkinDefinition skin))
        {
            Debug.LogWarning("[SHOP] Cannot refund - no current skin");
            return;
        }
        
        Debug.Log($"[SHOP] Current skin: {GetSkinId(skin)}, Owned: {IsSkinOwned(skin)}, UnlockedByDefault: {skin.unlockedByDefault}");
        
        if (!IsSkinOwned(skin))
        {
            Debug.LogWarning("[SHOP] Cannot refund - skin not owned");
            return;
        }
        
        if (skin.unlockedByDefault)
        {
            Debug.LogWarning("[SHOP] Cannot refund - skin is unlocked by default (free skin)");
            return;
        }

        string id = GetSkinId(skin);
        
        Debug.Log($"[SHOP] Refunding skin: {id}, price: {skin.price}");
        Debug.Log($"[SHOP] Current currency before refund: {GameData.GetCurrency()}");

        // Atomic refund via GameData - removes skin and adds currency back
        bool success = GameData.RefundSkin(id, skin.price);
        if (!success)
        {
            Debug.Log($"[SHOP] Refund FAILED for skin: {id}");
            return;
        }

        Debug.Log($"[SHOP] Refund SUCCESS! New currency: {GameData.GetCurrency()}");

        // Update local cache & UI
        purchasedSkinIds.Remove(id);

        // Ensure PlayerWallet reflects new value
        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.SetCurrency(GameData.GetCurrency());
        }

        onSkinRefunded?.Invoke(skin);
        RefreshUI();
    }

    // --- INTERNAL STATE ---
    private void SubscribeToWallet()
    {
        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.CurrencyChanged += HandleCurrencyChanged;
            // Immediately update currency label with current value
            HandleCurrencyChanged(PlayerWallet.Instance.CurrentCurrency);
        }
    }

    private void UnsubscribeFromWallet()
    {
        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.CurrencyChanged -= HandleCurrencyChanged;
        }
    }

    private void HandleCurrencyChanged(int amount)
    {
        Debug.Log($"[SHOP] HandleCurrencyChanged called with amount: {amount}");
        
        if (currencyLabel != null)
        {
            currencyLabel.text = amount.ToString();
            Debug.Log($"[SHOP] Currency label updated to: {amount}");
        }
        else
        {
            Debug.LogWarning("[SHOP] currencyLabel is NULL!");
        }

        UpdateBuyButtonState();
    }

    private void LoadState()
    {
        purchasedSkinIds.Clear();
        
        // Add default unlocked skins to GameData if not already there
        foreach (SkinDefinition skin in skins)
        {
            if (skin != null && skin.unlockedByDefault)
            {
                string skinId = GetSkinId(skin);
                GameData.AddOwnedSkin(skinId); // Idempotent
            }
        }

        // Load all owned skins from GameData
        List<string> ownedSkins = GameData.GetOwnedSkins();
        Debug.Log($"[SHOP] LoadState: GameData has {ownedSkins.Count} owned skins: {string.Join(", ", ownedSkins)}");
        
        foreach (string skinId in ownedSkins)
        {
            purchasedSkinIds.Add(skinId);
        }
        
        Debug.Log($"[SHOP] LoadState: Local cache now has {purchasedSkinIds.Count} skins");
        
        // Show what ID each skin in catalog generates
        for (int i = 0; i < skins.Count; i++)
        {
            if (skins[i] != null)
            {
                string catalogId = GetSkinId(skins[i]);
                bool owned = purchasedSkinIds.Contains(catalogId);
                Debug.Log($"[SHOP] Catalog[{i}]: displayName='{skins[i].displayName}', id='{skins[i].id}', generated ID='{catalogId}', owned={owned}");
            }
        }
    }

    private bool TryGetCurrentSkin(out SkinDefinition skin)
    {
        skin = null;
        if (skins.Count == 0) return false;
        currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, skins.Count - 1));
        skin = skins[currentIndex];
        return skin != null;
    }

    private bool IsSkinOwned(SkinDefinition skin)
    {
        if (skin == null) return false;
        if (skin.unlockedByDefault) return true;
        
        string skinId = GetSkinId(skin);
        // Check both local cache AND GameData for consistency
        bool inCache = purchasedSkinIds.Contains(skinId);
        bool inGameData = GameData.HasSkin(skinId);
        
        if (inCache != inGameData)
        {
            Debug.LogWarning($"[SHOP] Ownership mismatch for '{skinId}': cache={inCache}, GameData={inGameData}. Using GameData as source of truth.");
            // Sync the cache with GameData
            if (inGameData)
            {
                purchasedSkinIds.Add(skinId);
            }
            else
            {
                purchasedSkinIds.Remove(skinId);
            }
        }
        
        return inGameData;
    }

    private string GetSkinId(SkinDefinition skin)
    {
        if (skin == null) return string.Empty;
        if (!string.IsNullOrWhiteSpace(skin.id))
        {
            return skin.id.Trim();
        }

        if (!string.IsNullOrWhiteSpace(skin.displayName))
        {
            return skin.displayName.Trim();
        }

        int index = skins.IndexOf(skin);
        return index >= 0 ? $"skin_{index}" : skin.GetHashCode().ToString();
    }

    private string GetDisplayName(SkinDefinition skin)
    {
        if (skin == null) return "-";
        return string.IsNullOrWhiteSpace(skin.displayName) ? GetSkinId(skin) : skin.displayName;
    }

    public void RefreshUI()
    {
        if (skins.Count == 0)
        {
            ApplyEmptyState();
            return;
        }

        SkinDefinition current = skins[currentIndex];
        if (skinNameLabel != null)
        {
            skinNameLabel.text = GetDisplayName(current);
        }

        if (previousButton != null)
        {
            previousButton.interactable = skins.Count > 1;
        }

        if (nextButton != null)
        {
            nextButton.interactable = skins.Count > 1;
        }

        UpdateCarouselSprites();
        UpdateCurrencyLabel();
        UpdatePriceLabel(current);
        UpdateBuyButtonState();
        UpdateRefundButtonState(current);
    }

    private void ApplyEmptyState()
    {
        if (skinNameLabel != null) skinNameLabel.text = "No skins";
        if (priceLabel != null) priceLabel.text = "-";
        UpdateCurrencyLabel(); // Use the same method for consistency
        SetPreviewImage(leftPreview, null);
        SetPreviewImage(centerPreview, null);
        SetPreviewImage(rightPreview, null);
        if (buyButton != null) buyButton.interactable = false;
        if (refundButton != null) refundButton.interactable = false;
        if (previewButton != null) previewButton.interactable = false;
        if (previousButton != null) previousButton.interactable = false;
        if (nextButton != null) nextButton.interactable = false;
    }

    private void UpdateCurrencyLabel()
    {
        if (currencyLabel == null)
        {
            Debug.LogWarning("[SHOP] UpdateCurrencyLabel: currencyLabel is NULL!");
            return;
        }
        
        // Prefer PlayerWallet for UI consistency, fallback to GameData
        if (PlayerWallet.Instance != null)
        {
            int walletCurrency = PlayerWallet.Instance.CurrentCurrency;
            currencyLabel.text = walletCurrency.ToString();
            Debug.Log($"[SHOP] Currency label updated from PlayerWallet: {walletCurrency}");
        }
        else
        {
            int dataCurrency = GameData.GetCurrency();
            currencyLabel.text = dataCurrency.ToString();
            Debug.Log($"[SHOP] Currency label updated from GameData: {dataCurrency}");
        }
    }

    private void UpdatePriceLabel(SkinDefinition current)
    {
        if (priceLabel == null) return;

        if (current == null)
        {
            priceLabel.text = "-";
            return;
        }

        bool owned = IsSkinOwned(current);
        priceLabel.text = owned ? "Posiadane" : $"Cena: {current.price}";
    }

    private void UpdateBuyButtonState()
    {
        if (buyButton == null) return;
        if (!TryGetCurrentSkin(out SkinDefinition skin))
        {
            buyButton.interactable = false;
            return;
        }

        bool owned = IsSkinOwned(skin);
        buyButton.interactable = !owned && CanAfford(skin);

        if (buyButtonLabel != null)
        {
            buyButtonLabel.text = owned ? "Posiadane" : $"Kup ({skin.price})";
        }
    }

    private void UpdateRefundButtonState(SkinDefinition skin)
    {
        if (refundButton == null) return;

        bool owned = IsSkinOwned(skin);
        bool isEquipped = skin != null && GameData.GetEquippedSkin() == GetSkinId(skin);

        // przycisk "Equip" ma sens tylko dla posiadanych
        refundButton.gameObject.SetActive(owned);

        // jeœli ju¿ wyposa¿ony, mo¿na zablokowaæ klikanie
        refundButton.interactable = owned && !isEquipped;

        if (refundButtonLabel != null)
        {
            refundButtonLabel.text = isEquipped ? "Equipped" : "Equip";
        }
    }

    private bool CanAfford(SkinDefinition skin)
    {
        if (skin == null) return false;
        if (PlayerWallet.Instance == null) return false;
        return PlayerWallet.Instance.CurrentCurrency >= skin.price;
    }

    private void UpdateCarouselSprites()
    {
        if (skins.Count == 0)
        {
            SetPreviewImage(leftPreview, null);
            SetPreviewImage(centerPreview, null);
            SetPreviewImage(rightPreview, null);
            return;
        }

        SetPreviewImage(centerPreview, skins[currentIndex]);
        SetPreviewImage(leftPreview, skins[WrapIndex(currentIndex - 1)]);
        SetPreviewImage(rightPreview, skins[WrapIndex(currentIndex + 1)]);
    }

    private void SetPreviewImage(Image target, SkinDefinition skin)
    {
        if (target == null) return;
        target.sprite = skin != null ? skin.previewSprite : null;
        target.enabled = skin != null && skin.previewSprite != null;
    }

    public void EquipCurrentSkin()
    {
        Debug.Log("[SHOP] Equip button clicked!");

        if (!TryGetCurrentSkin(out SkinDefinition skin))
        {
            Debug.LogWarning("[SHOP] Cannot equip - no current skin");
            return;
        }

        if (!IsSkinOwned(skin))
        {
            Debug.LogWarning("[SHOP] Cannot equip - skin not owned");
            return;
        }

        string id = GetSkinId(skin);
        Debug.Log($"[SHOP] Equipping skin: {id}");

        GameData.SetEquippedSkin(id);
        Debug.Log($"[SHOP] EquippedSkin in GameData is now: {GameData.GetEquippedSkin()}");

        RefreshUI();
        
    }
    private int WrapIndex(int index)
    {
        if (skins.Count == 0) return 0;
        if (index < 0)
        {
            index = skins.Count - 1;
        }
        else if (index >= skins.Count)
        {
            index = 0;
        }
        return index;
    }
}
