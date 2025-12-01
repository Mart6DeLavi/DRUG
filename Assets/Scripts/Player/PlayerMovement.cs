using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    public event Action OnJump;

    [Header("Movement Settings")]
    public float speed = 5f;

    [Header("Jump Mode Toggles")]
    public bool mouseAimedJumpEnabled = true;
    public bool chargedJumpEnabled = true;

    [Header("Charged Jump Settings")]
    public float minJumpForce = 6f;
    public float maxJumpForce = 12f;
    public float maxChargeTime = 1.2f;

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

    [Header("Trajectory")]
    public LineRenderer trajectoryLine;
    public bool enableGroundTrajectory = true;
    public bool enableAirTrajectory = true;
    public bool alwaysShowGroundTrajectory = true;
    public float trajectorySimulationTime = 1.5f;
    public int trajectorySegments = 25;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;

    private bool isCharging;
    private bool chargeValid;
    private float chargeTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ClearLine();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        if (GameManager.Instance != null && GameManager.Instance.controlsReversed)
            moveInput *= -1f;

        bool canJump = isGrounded ||
                       (GameManager.Instance != null &&
                        GameManager.Instance.doubleJumpActive &&
                        extraJumpAvailable);

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
                chargeTimer += Time.deltaTime;
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

        isGrounded = IsGrounded();

        if (isGrounded)
            extraJumpAvailable = true;
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
                return;
            }

            DrawAirTrajectory();
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
            float t = dt * i;
            Vector2 pos = startPos + initialVel * t + 0.5f * gravity * t * t;
            trajectoryLine.SetPosition(i, pos);
            used++;

            bool hit = Physics2D.OverlapCircle(pos, groundCheckRadius, groundLayer);
            if (hit)
                break;
        }

        trajectoryLine.positionCount = used;
        trajectoryLine.enabled = true;
    }

    private void ClearLine()
    {
        if (trajectoryLine == null) return;
        trajectoryLine.positionCount = 0;
        trajectoryLine.enabled = false;
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

    // Double jump flag from original code
    public bool extraJumpAvailable = true;
}
