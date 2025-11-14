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
    }

    public void AddPoints(int basePoints)
    {
        score += Mathf.RoundToInt(basePoints * scoreMultiplier);
    }

    public void ActivateDoublePoints(float duration)
    {
        scoreMultiplier = 2f;
        bonusTime = duration;
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
    }
}
