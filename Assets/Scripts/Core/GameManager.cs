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
        if ( playerSpeedMultiplier < 1f)
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
            doubleJumpActive;

        bool anyDebuff =
            controlsReversed ||
            randomImpulsActive ||
            playerSpeedMultiplier < 1f;
            
        if (!anyBuff && !anyDebuff)
        {
            EffectFrameUI.Instance?.HideFrame();
        }
        else
        {   if (anyBuff)
                EffectFrameUI.Instance?.ShowBuffFrame();
            else if (anyDebuff)
                EffectFrameUI.Instance?.ShowDebuffFrame();
        }
    }

    #region Buffes
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

        if (BuffIconsManager.Instance != null && lowVisibilitySprite != null)
            BuffIconsManager.Instance.ShowEffectIcon(lowVisibilitySprite, duration);
    }

    #endregion
}
