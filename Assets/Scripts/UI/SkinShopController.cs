using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// Handles the UI logic for the skin shop: browsing, buying, previewing and equipping skins.
/// </summary>
public class SkinShopController : MonoBehaviour
{
    private const string PurchasedKey = "SkinShop_Purchased";

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
        Debug.Log($"SkinShopController OnEnable on {name}: skins count before load = {skins?.Count ?? -1}");
        SubscribeToWallet();
        LoadState();
        Debug.Log($"SkinShopController OnEnable on {name}: skins count after load = {skins?.Count ?? -1}");
        RefreshUI();
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
        if (!TryGetCurrentSkin(out SkinDefinition skin)) return;
        if (IsSkinOwned(skin)) return;

        if (PlayerWallet.Instance == null)
        {
            Debug.LogWarning("PlayerWallet is missing from the scene. Cannot process purchase.");
            return;
        }

        if (!PlayerWallet.Instance.TrySpendCurrency(skin.price))
        {
            Debug.Log("Not enough currency to buy: " + GetDisplayName(skin));
            return;
        }

        purchasedSkinIds.Add(GetSkinId(skin));
        SavePurchased();
        RefreshUI();
    }

    public void PreviewCurrentSkin()
    {
        if (!TryGetCurrentSkin(out SkinDefinition skin)) return;
        onSkinPreview?.Invoke(skin);
    }

    public void RefundCurrentSkin()
    {
        if (!TryGetCurrentSkin(out SkinDefinition skin)) return;
        if (!IsSkinOwned(skin)) return;
        if (skin.unlockedByDefault) return; // Cannot remove default skins.

        string id = GetSkinId(skin);
        if (!purchasedSkinIds.Remove(id))
        {
            return;
        }

        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.AddCurrency(skin.price);
        }

        SavePurchased();
        onSkinRefunded?.Invoke(skin);
        RefreshUI();
    }

    // --- INTERNAL STATE ---
    private void SubscribeToWallet()
    {
        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.CurrencyChanged += HandleCurrencyChanged;
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
        if (currencyLabel != null)
        {
            currencyLabel.text = amount.ToString();
        }

        UpdateBuyButtonState();
    }

    private void LoadState()
    {
        purchasedSkinIds.Clear();
        foreach (SkinDefinition skin in skins)
        {
            if (skin != null && skin.unlockedByDefault)
            {
                purchasedSkinIds.Add(GetSkinId(skin));
            }
        }

        string saved = PlayerPrefs.GetString(PurchasedKey, string.Empty);
        if (!string.IsNullOrEmpty(saved))
        {
            string[] ids = saved.Split('|', StringSplitOptions.RemoveEmptyEntries);
            foreach (string id in ids)
            {
                purchasedSkinIds.Add(id);
            }
        }

    }

    private void SavePurchased()
    {
        string serialized = string.Join("|", purchasedSkinIds);
        PlayerPrefs.SetString(PurchasedKey, serialized);
        PlayerPrefs.Save();
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
        return purchasedSkinIds.Contains(GetSkinId(skin));
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

    private void RefreshUI()
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
        if (skinNameLabel != null) skinNameLabel.text = "Brak skinów";
        if (priceLabel != null) priceLabel.text = "-";
        if (currencyLabel != null) currencyLabel.text = PlayerWallet.Instance != null ? PlayerWallet.Instance.CurrentCurrency.ToString() : "--";
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
        if (currencyLabel == null) return;
        currencyLabel.text = PlayerWallet.Instance != null ? PlayerWallet.Instance.CurrentCurrency.ToString() : "--";
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
        if (refundButton == null)
        {
            return;
        }

        bool owned = IsSkinOwned(skin);
        bool canRefund = owned && skin != null && !skin.unlockedByDefault;
        refundButton.interactable = canRefund;
        refundButton.gameObject.SetActive(owned);

        if (refundButtonLabel != null)
        {
            refundButtonLabel.text = canRefund ? "Ubierz" : (owned ? "Zablokowane" : "Brak");
        }

        if (previewButton != null)
        {
            previewButton.interactable = skin != null;
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
