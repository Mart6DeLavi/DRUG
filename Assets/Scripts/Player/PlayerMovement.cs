using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public event Action OnJump;

    [Header("VFX (optional)")]
    [Tooltip("Effect played when the player jumps.")]
    public ParticleSystem jumpEffect;

    [Tooltip("Effect played when the player lands on the ground.")]
    public ParticleSystem landEffect;

    [Tooltip("Effect played when the player dies.")]
    public ParticleSystem deathEffect;

    [Tooltip("Where to spawn jump/land dust (e.g. GroundCheck_Center). If null, uses player position.")]
    public Transform vfxFeetPoint;

    private bool pendingLandVfx = false;
    private float lastJumpTime = -999f;
    private bool wasGrounded = false;

    [Tooltip("Small delay so landing VFX won't trigger in the same frame as jump.")]
    [SerializeField] private float landVfxMinDelay = 0.05f;

    [Header("Death Flow")]
    [Tooltip("How long to freeze the game (real seconds) to show death VFX before loading GameOverScene.")]
    public float deathFreezeSeconds = 2f;

    [Tooltip("Scene name to load after death.")]
    public string gameOverSceneName = "GameOverScene";

    private bool isDying = false;

    [Header("Movement Settings")]
    public float speed = 5f;

    [Header("Jump Mode Toggles")]
    public bool mouseAimedJumpEnabled = true;
    public bool chargedJumpEnabled = true;

    [Header("Charged Jump Settings")]
    public float minJumpForce = 6f;
    public float maxJumpForce = 12f;
    public float maxChargeTime = 1.2f;
    [Tooltip("Charge speed multiplier while charging the DOUBLE JUMP in air (e.g. 2 = 2x faster).")]
    public float airDoubleJumpChargeMultiplier = 2.5f;

    [Header("Non-Charged Jump Settings")]
    public float baseMouseJumpForce = 10f;
    public float verticalJumpForce = 10f;

    [Header("Physics")]
    public float moveAcceleration = 25f;
    public float maxHorizontalSpeed = 8f;
    public float minUpwardY = 0.2f;
    public float maxJumpAngleFromUp = 65f;

    [Header("Ground Check")]
    public Transform[] groundChecks;
    public float groundCheckRadius = 0.18f;
    public LayerMask groundLayer;
    [Tooltip("How long (in seconds) ground contact must be detected before considering the player grounded. Helps prevent trajectory flicker.")]
    public float groundedConfirmTime = 0.05f;

    [Header("Trajectory")]
    public LineRenderer trajectoryLine;          // obecna trajektoria (bez skoku)
    public LineRenderer chargedTrajectoryLine;   // NOWA: trajektoria skoku podczas charge w powietrzu

    public bool enableGroundTrajectory = true;
    public bool enableAirTrajectory = true;
    public bool alwaysShowGroundTrajectory = true;
    public float trajectorySimulationTime = 1.5f;
    public int trajectorySegments = 25;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float groundedConfirmTimer;
    private float moveInput;

    private bool isCharging;
    private bool chargeValid;
    private float chargeTimer;

    // Double jump flag from original code
    public bool extraJumpAvailable = true;
    public bool IsGroundedAnim => isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // IMPORTANT: do this in Awake so particles don't fire on scene start
        SetupVfx();
    }

    void Start()
    {
        // Hierarchy note: keep both LineRenderers as children of the player
        if (trajectoryLine != null && trajectoryLine.transform.parent != transform)
            trajectoryLine.transform.SetParent(transform, true);

        if (chargedTrajectoryLine != null && chargedTrajectoryLine.transform.parent != transform)
            chargedTrajectoryLine.transform.SetParent(transform, true);

        ClearLine();
        ClearLine(chargedTrajectoryLine);
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        if (GameManager.Instance != null && GameManager.Instance.controlsReversed)
            moveInput *= -1f;

        // Keep grounded state in sync for visuals (trajectory) and input logic.
        bool rawGrounded = IsGrounded();
        if (rawGrounded)
        {
            groundedConfirmTimer += Time.deltaTime;
            if (groundedConfirmTimer >= groundedConfirmTime)
            {
                isGrounded = true;
                extraJumpAvailable = true;
            }
        }
        else
        {
            groundedConfirmTimer = 0f;
            isGrounded = false;
        }

        bool canJump = isGrounded ||
                       (GameManager.Instance != null &&
                        GameManager.Instance.doubleJumpActive &&
                        extraJumpAvailable);

        // Landing VFX: trigger once when transitioning from air -> grounded after a jump
        if (!wasGrounded && isGrounded)
        {
            if (pendingLandVfx && landEffect != null && (Time.time - lastJumpTime >= landVfxMinDelay))
            {
                pendingLandVfx = false;
                Vector2 p = vfxFeetPoint ? (Vector2)vfxFeetPoint.position : (Vector2)transform.position;
                PlayBurst(landEffect, p);
            }
        }
        wasGrounded = isGrounded;

        // ------------------------
        //      JUMP LOGIC
        // ------------------------
        if (chargedJumpEnabled)
        {
            if (Input.GetButtonDown("Jump"))
            {
                if (canJump)
                {
                    isCharging = true;
                    chargeValid = true;
                    chargeTimer = 0f;
                }
                else
                {
                    isCharging = false;
                    chargeValid = false;
                }
            }

            if (isCharging && chargeValid)
            {
                float chargeMult = 1f;

                // Faster charge only when charging the SECOND jump (double jump) in the air
                if (!isGrounded && GameManager.Instance?.doubleJumpActive == true && extraJumpAvailable)
                    chargeMult = airDoubleJumpChargeMultiplier;

                chargeTimer += Time.deltaTime * chargeMult;

                if (chargeTimer > maxChargeTime)
                    chargeTimer = maxChargeTime;
            }

            if (Input.GetButtonUp("Jump"))
            {
                if (isCharging && chargeValid)
                    DoChargedJump();

                isCharging = false;
                chargeValid = false;
                chargeTimer = 0f;
            }
        }
        else
        {
            if (Input.GetButtonDown("Jump") && canJump)
                DoSimpleJump();
        }

        // ------------------------
        //   TRAJECTORY DRAWING
        // ------------------------
        UpdateTrajectory();
    }

    void FixedUpdate()
    {
        ApplyHorizontalMovement();
    }

    // ----------------------------------------------------------
    //                     HORIZONTAL MOVEMENT
    // ----------------------------------------------------------
    private void ApplyHorizontalMovement()
    {
        float multiplier = 1f;
        float globalMult = 1f;
        float tempoMult = 1f;

        if (GameManager.Instance != null)
            multiplier = GameManager.Instance.playerSpeedMultiplier;
        if (GameSpeedController.Instance != null)
            globalMult = GameSpeedController.Instance.CurrentMultiplier;
        if (TempoEffectController.Instance != null)
            tempoMult = TempoEffectController.Instance.CurrentTempoMultiplier;

        float effectiveMaxSpeed = maxHorizontalSpeed * multiplier * globalMult * tempoMult;
        float effectiveAccel = moveAcceleration * multiplier * globalMult * tempoMult;

        rb.AddForce(Vector2.right * (moveInput * effectiveAccel), ForceMode2D.Force);

        Vector2 vel = rb.linearVelocity;
        if (Mathf.Abs(vel.x) > effectiveMaxSpeed)
            vel.x = Mathf.Sign(vel.x) * effectiveMaxSpeed;

        rb.linearVelocity = new Vector2(vel.x, rb.linearVelocity.y);
    }

    private float GetEffectiveMaxHorizontalSpeed()
    {
        float m = 1f;
        float g = 1f;
        float t = 1f;

        if (GameManager.Instance != null)
            m = GameManager.Instance.playerSpeedMultiplier;
        if (GameSpeedController.Instance != null)
            g = GameSpeedController.Instance.CurrentMultiplier;
        if (TempoEffectController.Instance != null)
            t = TempoEffectController.Instance.CurrentTempoMultiplier;

        return maxHorizontalSpeed * m * g * t;
    }

    private Vector2 ClampHorizontal(Vector2 v)
    {
        float max = GetEffectiveMaxHorizontalSpeed();
        if (Mathf.Abs(v.x) > max)
            v.x = Mathf.Sign(v.x) * max;
        return v;
    }

    // ----------------------------------------------------------
    //                            JUMPS
    // ----------------------------------------------------------
    private void DoChargedJump()
    {
        float t = Mathf.Clamp01(chargeTimer / maxChargeTime);
        float force = Mathf.Lerp(minJumpForce, maxJumpForce, t);

        Vector2 dir = mouseAimedJumpEnabled ? GetMouseDir() : Vector2.up;
        Vector2 vel = ClampHorizontal(dir * force);

        rb.linearVelocity = vel;
        OnJump?.Invoke();

        // VFX (jump)
        lastJumpTime = Time.time;
        pendingLandVfx = true;
        PlayBurst(jumpEffect, vfxFeetPoint ? (Vector2)vfxFeetPoint.position : (Vector2)transform.position);

        if (!isGrounded && GameManager.Instance?.doubleJumpActive == true)
            extraJumpAvailable = false;
    }

    private void DoSimpleJump()
    {
        Vector2 vel;

        if (mouseAimedJumpEnabled)
        {
            Vector2 dir = GetMouseDir();
            vel = ClampHorizontal(dir * baseMouseJumpForce);
        }
        else
        {
            vel = ClampHorizontal(new Vector2(rb.linearVelocity.x, verticalJumpForce));
        }

        rb.linearVelocity = vel;
        OnJump?.Invoke();

        // VFX (jump)
        lastJumpTime = Time.time;
        pendingLandVfx = true;
        PlayBurst(jumpEffect, vfxFeetPoint ? (Vector2)vfxFeetPoint.position : (Vector2)transform.position);

        if (!isGrounded && GameManager.Instance?.doubleJumpActive == true)
            extraJumpAvailable = false;
    }

    private Vector2 GetMouseDir()
    {
        if (Camera.main == null)
            return Vector2.up;

        Vector3 mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 raw = (Vector2)(mw - transform.position);

        if (raw.sqrMagnitude < 0.0001f)
            raw = Vector2.up;

        if (raw.y < minUpwardY)
            raw.y = minUpwardY;

        Vector2 dir = raw.normalized;

        float angle = Vector2.Angle(Vector2.up, dir);
        if (angle > maxJumpAngleFromUp)
        {
            float side = Mathf.Sign(dir.x);
            if (side == 0) side = 1;

            float rad = maxJumpAngleFromUp * side * Mathf.Deg2Rad;
            dir = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
        }

        return dir.normalized;
    }

    // ----------------------------------------------------------
    //                     TRAJECTORY HANDLING
    // ----------------------------------------------------------
    private void UpdateTrajectory()
    {
        if (trajectoryLine == null)
            return;

        if (isGrounded)
        {
            ClearLine(chargedTrajectoryLine);

            if (!enableGroundTrajectory)
            {
                ClearLine();
                return;
            }

            DrawGroundTrajectory();
        }
        else
        {
            if (!enableAirTrajectory)
            {
                ClearLine();
                ClearLine(chargedTrajectoryLine);
                return;
            }

            DrawAirTrajectory();

            if (chargedTrajectoryLine != null && isCharging && chargeValid)
            {
                float t = Mathf.Clamp01(chargeTimer / maxChargeTime);
                float force = Mathf.Lerp(minJumpForce, maxJumpForce, t);

                Vector2 dir = mouseAimedJumpEnabled ? GetMouseDir() : Vector2.up;
                Vector2 jumpVel = ClampHorizontal(dir * force);

                SimulateTrajectory(chargedTrajectoryLine, rb.position, jumpVel);
            }
            else
            {
                ClearLine(chargedTrajectoryLine);
            }
        }
    }

    private void DrawGroundTrajectory()
    {
        if (!chargedJumpEnabled)
        {
            Vector2 start = rb.position;
            Vector2 initial = mouseAimedJumpEnabled ?
                ClampHorizontal(GetMouseDir() * baseMouseJumpForce) :
                ClampHorizontal(new Vector2(rb.linearVelocity.x, verticalJumpForce));

            SimulateTrajectory(start, initial);
            return;
        }

        float t;
        if (isCharging && chargeValid)
        {
            t = Mathf.Clamp01(chargeTimer / maxChargeTime);
        }
        else
        {
            if (!alwaysShowGroundTrajectory)
            {
                ClearLine();
                return;
            }
            t = 0f;
        }

        float force = Mathf.Lerp(minJumpForce, maxJumpForce, t);
        Vector2 dir = mouseAimedJumpEnabled ? GetMouseDir() : Vector2.up;
        Vector2 initialVel = ClampHorizontal(dir * force);

        SimulateTrajectory(rb.position, initialVel);
    }

    private void DrawAirTrajectory()
    {
        Vector2 start = rb.position;
        Vector2 initial = ClampHorizontal(rb.linearVelocity);

        SimulateTrajectory(start, initial);
    }

    private void SimulateTrajectory(Vector2 startPos, Vector2 initialVel)
    {
        Vector2 gravity = Physics2D.gravity * rb.gravityScale;
        int maxSeg = Mathf.Max(2, trajectorySegments);

        trajectoryLine.positionCount = maxSeg;

        float dt = trajectorySimulationTime / (maxSeg - 1);
        int used = 0;

        for (int i = 0; i < maxSeg; i++)
        {
            float tt = dt * i;
            Vector2 pos = startPos + initialVel * tt + 0.5f * gravity * tt * tt;

            if (i > 0)
            {
                Vector2 prev = (Vector2)trajectoryLine.GetPosition(i - 1);
                Vector2 delta = pos - prev;
                float dist = delta.magnitude;

                if (dist > 0.0001f)
                {
                    RaycastHit2D hit = Physics2D.CircleCast(prev, groundCheckRadius, delta / dist, dist, groundLayer);
                    if (hit.collider != null)
                    {
                        trajectoryLine.SetPosition(i, hit.point);
                        used++;
                        break;
                    }
                }
            }

            trajectoryLine.SetPosition(i, pos);
            used++;
        }

        trajectoryLine.positionCount = used;
        trajectoryLine.enabled = true;
    }

    private void SimulateTrajectory(LineRenderer line, Vector2 startPos, Vector2 initialVel)
    {
        if (line == null) return;

        Vector2 gravity = Physics2D.gravity * rb.gravityScale;
        int maxSeg = Mathf.Max(2, trajectorySegments);

        line.positionCount = maxSeg;

        float dt = trajectorySimulationTime / (maxSeg - 1);
        int used = 0;

        for (int i = 0; i < maxSeg; i++)
        {
            float tt = dt * i;
            Vector2 pos = startPos + initialVel * tt + 0.5f * gravity * tt * tt;

            if (i > 0)
            {
                Vector2 prev = (Vector2)line.GetPosition(i - 1);
                Vector2 delta = pos - prev;
                float dist = delta.magnitude;

                if (dist > 0.0001f)
                {
                    RaycastHit2D hit = Physics2D.CircleCast(prev, groundCheckRadius, delta / dist, dist, groundLayer);
                    if (hit.collider != null)
                    {
                        line.SetPosition(i, hit.point);
                        used++;
                        break;
                    }
                }
            }

            line.SetPosition(i, pos);
            used++;
        }

        line.positionCount = used;
        line.enabled = true;
    }

    private void ClearLine()
    {
        if (trajectoryLine == null) return;
        trajectoryLine.positionCount = 0;
        trajectoryLine.enabled = false;
    }

    private void ClearLine(LineRenderer line)
    {
        if (line == null) return;
        line.positionCount = 0;
        line.enabled = false;
    }

    // ----------------------------------------------------------
    //                     GROUND CHECK
    // ----------------------------------------------------------
    private bool IsGrounded()
    {
        foreach (var p in groundChecks)
        {
            if (!p) continue;
            if (Physics2D.OverlapCircle(p.position, groundCheckRadius, groundLayer))
                return true;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundChecks == null) return;

        Gizmos.color = Color.yellow;
        foreach (Transform p in groundChecks)
        {
            if (!p) continue;
            Gizmos.DrawWireSphere(p.position, groundCheckRadius);
        }
    }

    // ---------------- VFX setup & playback ----------------
    private void SetupVfx()
    {
        SetupOne(jumpEffect, useUnscaledTime: false);
        SetupOne(landEffect, useUnscaledTime: false);
        SetupOne(deathEffect, useUnscaledTime: true); // so it runs during timeScale=0
    }

    private void SetupOne(ParticleSystem ps, bool useUnscaledTime)
    {
        if (ps == null) return;

        // Make sure the object is active (StopAction Disable can break future plays)
        if (!ps.gameObject.activeSelf)
            ps.gameObject.SetActive(true);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.useUnscaledTime = useUnscaledTime;

        // CRITICAL: prevent the system from disabling itself after playing once
        main.stopAction = ParticleSystemStopAction.None;

        // Clear any particles that spawned on scene load
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Clear(true);
    }

    private void PlayBurst(ParticleSystem ps, Vector2 pos)
    {
        if (ps == null) return;

        if (!ps.gameObject.activeSelf)
            ps.gameObject.SetActive(true);

        ps.transform.position = new Vector3(pos.x, pos.y, ps.transform.position.z);

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Clear(true);
        ps.Play(true);
    }

    // Called from PlayerDeath
    public void TriggerDeath()
    {
        if (isDying) return;
        isDying = true;
        StartCoroutine(DeathRoutine());
    }

    private System.Collections.IEnumerator DeathRoutine()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        PlayBurst(deathEffect, transform.position);

        float prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(deathFreezeSeconds);
        Time.timeScale = prevTimeScale;

        if (SurvivalScore.Instance != null)
            SurvivalScore.Instance.SealFinalScore();

        SceneManager.LoadScene(gameOverSceneName);
    }

    // Backward-compatible
    public void PlayDeathVfx()
    {
        TriggerDeath();
    }
}