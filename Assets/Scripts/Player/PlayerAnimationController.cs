using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animator parameters")]
    public string speedXParamName = "SpeedX";
    public string isGroundedParam = "IsGrounded";

    [Tooltip("Speed threshold at which we consider player is moving.")]
    public float moveThreshold = 0.1f;

    [Header("Skórki gracza")]
    public SkinShopController.SkinDefinition[] allSkins;

    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerMovement movement;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<PlayerMovement>();

        // ustaw sprite i animator skina
        string equippedId = GameData.GetEquippedSkin();
        if (!string.IsNullOrEmpty(equippedId))
        {
            foreach (var skin in allSkins)
            {
                if (GetSkinIdFromDef(skin) == equippedId)
                {
                    if (skin.previewSprite != null)
                        spriteRenderer.sprite = skin.previewSprite;


                    if (skin.animatorController != null)
                        animator.runtimeAnimatorController = skin.animatorController;

                    break;
                }
            }
        }
    }

    private void Update()
    {
        float vx = rb.linearVelocity.x;

        animator.SetFloat(speedXParamName, Mathf.Abs(vx));
        animator.SetBool(isGroundedParam, movement.IsGroundedAnim);

        // flip
        if (vx > moveThreshold)
            spriteRenderer.flipX = false;
        else if (vx < -moveThreshold)
            spriteRenderer.flipX = true;
    }

    private string GetSkinIdFromDef(SkinShopController.SkinDefinition skin)
    {
        if (skin == null) return "";
        if (!string.IsNullOrWhiteSpace(skin.id)) return skin.id.Trim();
        if (!string.IsNullOrWhiteSpace(skin.displayName)) return skin.displayName.Trim();
        return skin.GetHashCode().ToString();
    }
}
