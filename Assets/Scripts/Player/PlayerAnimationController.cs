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

    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Bierzemy prędkość poziomą z Rigidbody
        float vx = rb.linearVelocity.x; // używamy linearVelocity, bo taki masz w PlayerMovement

        // Ustawiamy parametr w Animatorze (wartość bezwzględna)
        if (animator != null)
        {
            animator.SetFloat(speedXParamName, Mathf.Abs(vx));
        }

        // Obracamy sprite w zależności od kierunku ruchu
        if (spriteRenderer != null)
        {
            if (vx > moveThreshold)
            {
                // patrzymy w prawo
                spriteRenderer.flipX = false;
            }
            else if (vx < -moveThreshold)
            {
                // patrzymy w lewo
                spriteRenderer.flipX = true;
            }
            // jeśli |vx| < threshold – nie zmieniamy flipX, zostaje ostatni kierunek
        }
    }
}
