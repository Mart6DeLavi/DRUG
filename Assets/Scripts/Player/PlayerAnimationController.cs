using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerAnimationController : MonoBehaviour
{
    [Tooltip("Nazwa parametru w Animatorze odpowiadającego za prędkość poziomą.")]
    public string speedXParamName = "SpeedX";

    [Tooltip("Prędkość przy której uznajemy, że gracz się porusza.")]
    public float moveThreshold = 0.1f;

    [Header("Skórki gracza")]
    [Tooltip("Lista definicji skórek zgodna z katalogiem w sklepie.")]
    public SkinShopController.SkinDefinition[] allSkins;


    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private string GetSkinIdFromDef(SkinShopController.SkinDefinition skin)
    {
        if (skin == null) return "";
        if (!string.IsNullOrWhiteSpace(skin.id))
            return skin.id.Trim();
        if (!string.IsNullOrWhiteSpace(skin.displayName))
            return skin.displayName.Trim();

        int index = System.Array.IndexOf(allSkins, skin);
        return index >= 0 ? $"skin_{index}" : skin.GetHashCode().ToString();
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        string equippedId = GameData.GetEquippedSkin();

        Debug.Log($"[PLAYER] Awake, equipped skin id = '{equippedId}', allSkins length = {allSkins?.Length ?? 0}");

        if (!string.IsNullOrEmpty(equippedId) && allSkins != null && spriteRenderer != null)
        {
            bool found = false;
            foreach (var skin in allSkins)
            {
                string id = GetSkinIdFromDef(skin);
                Debug.Log($"[PLAYER] Check skin def id='{id}' (name='{skin.displayName}')");

                if (id == equippedId)
                {
                    if (skin.previewSprite != null)
                        spriteRenderer.sprite = skin.previewSprite;

                    if (skin.animatorController != null)
                        animator.runtimeAnimatorController = skin.animatorController;

                    found = true;
                    break;
                }
            }

            if (!found)
                Debug.LogWarning("[PLAYER] No matching skin definition found for id = '" + equippedId + "'");
        }
    }

    private void Update()
    {
        float vx = rb.linearVelocity.x;

        if (animator != null)
            animator.SetFloat(speedXParamName, Mathf.Abs(vx));

        if (spriteRenderer != null)
        {
            if (vx > moveThreshold)
                spriteRenderer.flipX = false;
            else if (vx < -moveThreshold)
                spriteRenderer.flipX = true;
        }
    }
}
