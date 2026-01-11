using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimationController : MonoBehaviour
{
    [Tooltip("Name of Animator parameter corresponding to horizontal speed.")]
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
        // Get horizontal velocity from Rigidbody
        float vx = rb.linearVelocity.x; // using linearVelocity as in PlayerMovement

        // Set parameter in Animator (absolute value)
        if (animator != null)
        {
            animator.SetFloat(speedXParamName, Mathf.Abs(vx));
        }

        // Flip sprite depending on movement direction
        if (spriteRenderer != null)
        {
            if (vx > moveThreshold)
            {
                // looking right
                spriteRenderer.flipX = false;
            }
            else if (vx < -moveThreshold)
            {
                // looking left
                spriteRenderer.flipX = true;
            }
            // if |vx| < threshold – don't change flipX, keep last direction
        }
    }
}
