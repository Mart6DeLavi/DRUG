using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int score = 0;
    private float scoreMultiplier = 1f;

    // Double Jump bonus
    public bool doubleJumpActive = false;
    private float doubleJumpBonusTime = 0f;

    private float bonusTime = 0f;

    // Multiplication factor for enemy speed
    public float enemySpeedMultiplier = 1f;

    // Player speed multiplier
    public float playerSpeedMultiplier = 1f;

    // Debuffs
    public bool controlsReversed = false;
    public bool randomImpulsActive = false;

    // Low visibility
    public GameObject lowVisibilityPanel;

    private float reverseDuration = 0f, impulsDuration = 0f, slowDuration = 0f, lowVisibilityDuration = 0f;

    // Sprites for UI
    public Sprite speedBuffSprite;
    public Sprite doubleJumpSprite;
    public Sprite doublePointsBuffSprite;

    public Sprite reverseControlsDebuffSprite;
    public Sprite randomImpulseSprite;
    public Sprite slowPlayerSprite;
    public Sprite lowVisibilitySprite;

    // ================= Random Impulse Debuff Settings =================
    [Header("Random Impulse Debuff Settings")]
    [SerializeField] private float randomImpulseMinForce = 2f;
    [SerializeField] private float randomImpulseMaxForce = 6f;

    [Tooltip("Co ile sekund ma być dodany impuls (jeśli random interval = false).")]
    [SerializeField] private float randomImpulseInterval = 0.35f;

    [Tooltip("Jeśli true, interwał będzie losowany z zakresu poniżej.")]
    [SerializeField] private bool randomImpulseUseRandomInterval = false;

    [Tooltip("Zakres losowania interwału (min/max), jeśli random interval = true.")]
    [SerializeField] private Vector2 randomImpulseIntervalRange = new Vector2(0.2f, 0.6f);

    public float RandomImpulseMinForce => randomImpulseMinForce;
    public float RandomImpulseMaxForce => randomImpulseMaxForce;
    public float RandomImpulseInterval => randomImpulseInterval;
    public bool RandomImpulseUseRandomInterval => randomImpulseUseRandomInterval;
    public Vector2 RandomImpulseIntervalRange => randomImpulseIntervalRange;
    // ================================================================

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (bonusTime > 0)
        {
            bonusTime -= Time.deltaTime;
            if (bonusTime <= 0)
                ResetBonuses();
        }

        if (doubleJumpActive && doubleJumpBonusTime > 0)
        {
            doubleJumpBonusTime -= Time.deltaTime;
            if (doubleJumpBonusTime <= 0)
                doubleJumpActive = false;
        }

        // Debuffs
        if (controlsReversed)
        {
            reverseDuration -= Time.deltaTime;
            if (reverseDuration <= 0)
                controlsReversed = false;
        }

        if (randomImpulsActive)
        {
            impulsDuration -= Time.deltaTime;
            if (impulsDuration <= 0)
                randomImpulsActive = false;
        }

        // Slow player
        if (playerSpeedMultiplier < 1f)
        {
            slowDuration -= Time.deltaTime;
            if (slowDuration <= 0)
                playerSpeedMultiplier = 1f;
        }

        // Low visibility
        if (lowVisibilityPanel != null && lowVisibilityPanel.activeSelf)
        {
            lowVisibilityDuration -= Time.deltaTime;
            if (lowVisibilityDuration <= 0)
                lowVisibilityPanel.SetActive(false);
        }

        UpdateEffectFrameVisibility();
    }

    void UpdateEffectFrameVisibility()
    {
        bool anyBuff =
            playerSpeedMultiplier > 1f ||
            doubleJumpActive ||
            scoreMultiplier > 1f ||
            enemySpeedMultiplier != 1f;

        bool anyDebuff =
            controlsReversed ||
            randomImpulsActive ||
            playerSpeedMultiplier < 1f ||
            (lowVisibilityPanel != null && lowVisibilityPanel.activeSelf);

        if (!anyBuff && !anyDebuff)
        {
            EffectFrameUI.Instance?.HideFrame();
        }
        else
        {
            if (anyBuff)
                EffectFrameUI.Instance?.ShowBuffFrame();
            else if (anyDebuff)
                EffectFrameUI.Instance?.ShowDebuffFrame();
        }
    }

    #region Buffs
    public void AddPoints(int basePoints)
    {
        score += Mathf.RoundToInt(basePoints * scoreMultiplier);
    }

    public void ActivateDoublePoints(float duration)
    {
        scoreMultiplier = 2f;
        bonusTime = duration;

        EffectFrameUI.Instance?.ShowBuffFrame();

        if (BuffIconsManager.Instance != null && doublePointsBuffSprite != null)
            BuffIconsManager.Instance.ShowEffectIcon(doublePointsBuffSprite, duration);
    }

    public void ActivateSlowEnemies(float duration, float slowMultiplier)
    {
        enemySpeedMultiplier = slowMultiplier;
        bonusTime = duration;
    }

    public void ActivateSpeedBoost(float duration, float speedMultiplier)
    {
        playerSpeedMultiplier = speedMultiplier;
        bonusTime = duration;

        EffectFrameUI.Instance?.ShowBuffFrame();

        if (BuffIconsManager.Instance != null && speedBuffSprite != null)
            BuffIconsManager.Instance.ShowEffectIcon(speedBuffSprite, duration);
    }

    private void ResetBonuses()
    {
        scoreMultiplier = 1f;
        enemySpeedMultiplier = 1f;
        playerSpeedMultiplier = 1f;
    }

    public void ActivateDoubleJump(float duration)
    {
        doubleJumpActive = true;
        doubleJumpBonusTime = duration;

        EffectFrameUI.Instance?.ShowBuffFrame();

        if (BuffIconsManager.Instance != null && doubleJumpSprite != null)
            BuffIconsManager.Instance.ShowEffectIcon(doubleJumpSprite, duration);
    }
    #endregion

    // Debuff methods
    #region Debuffs
    public void ActivateReverseControls(float duration)
    {
        controlsReversed = true;
        reverseDuration = duration;

        EffectFrameUI.Instance?.ShowDebuffFrame();

        if (BuffIconsManager.Instance != null && reverseControlsDebuffSprite != null)
            BuffIconsManager.Instance.ShowEffectIcon(reverseControlsDebuffSprite, duration);
    }

    public void ActivateRandomImpulse(float duration)
    {
        randomImpulsActive = true;
        impulsDuration = duration;

        EffectFrameUI.Instance?.ShowDebuffFrame();

        if (BuffIconsManager.Instance != null && randomImpulseSprite != null)
            BuffIconsManager.Instance.ShowEffectIcon(randomImpulseSprite, duration);
    }

    // Opcjonalnie: wersja z ustawianiem parametrów debuffa z pickupa
    public void ActivateRandomImpulse(float duration, float minForce, float maxForce, float interval)
    {
        randomImpulsActive = true;
        impulsDuration = duration;

        randomImpulseMinForce = minForce;
        randomImpulseMaxForce = maxForce;
        randomImpulseInterval = interval;

        EffectFrameUI.Instance?.ShowDebuffFrame();

        if (BuffIconsManager.Instance != null && randomImpulseSprite != null)
            BuffIconsManager.Instance.ShowEffectIcon(randomImpulseSprite, duration);
    }

    public void ActivateSlowPlayer(float duration, float slowMultiplier)
    {
        playerSpeedMultiplier = slowMultiplier;
        slowDuration = duration;

        EffectFrameUI.Instance?.ShowDebuffFrame();

        if (BuffIconsManager.Instance != null && slowPlayerSprite != null)
            BuffIconsManager.Instance.ShowEffectIcon(slowPlayerSprite, duration);
    }

    public void ActivateLimitedVisibility(float duration)
    {
        if (lowVisibilityPanel != null)
        {
            lowVisibilityPanel.SetActive(true);
            lowVisibilityDuration = duration;
        }

        EffectFrameUI.Instance?.ShowDebuffFrame();

        if (BuffIconsManager.Instance != null && lowVisibilitySprite != null)
            BuffIconsManager.Instance.ShowEffectIcon(lowVisibilitySprite, duration);
    }

    #endregion
}