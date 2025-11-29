using UnityEngine;

/// <summary>
/// Moves an obstacle back and forth within a small area, keeping it confined to the platform it belongs to.
/// Attach to the obstacle sprite and tweak the travel distances to define the motion rectangle in local space.
/// </summary>
[DisallowMultipleComponent]
public class DynamicObstacle : MonoBehaviour
{
    [Header("Motion")]
    [Tooltip("Total travel distance per axis in local space (units).")]
    [SerializeField] private Vector2 localTravel = new Vector2(1.5f, 0f);

    [Tooltip("Units per second used while ping-ponging along the travel path.")]
    [SerializeField] private float moveSpeed = 1.5f;

    [Tooltip("Optional phase offset (seconds) so multiple obstacles do not move in sync.")]
    [SerializeField] private float timeOffset = 0f;

    [Tooltip("Extra phase offset for the Y axis (seconds).")]
    [SerializeField] private float yAxisPhaseOffset = 0.5f;

    [Header("Bounds")] 
    [Tooltip("If enabled the obstacle never leaves the platform collider/render bounds.")]
    [SerializeField] private bool clampToPlatformBounds = true;

    [Tooltip("Optional collider that represents the platform bounds. If empty the first collider found on a parent will be used.")]
    [SerializeField] private Collider2D platformBoundsOverride;

    [Tooltip("Extra world-space padding kept between the obstacle edges and the platform bounds.")]
    [SerializeField] private Vector2 padding = new Vector2(0.05f, 0.05f);

    private Vector3 _startLocalPosition;
    private Collider2D _selfCollider;
    private SpriteRenderer _selfRenderer;
    private Collider2D _platformCollider;
    private SpriteRenderer _platformRenderer;
    private float _time;

    private void Awake()
    {
        _startLocalPosition = transform.localPosition;
        _selfCollider = GetComponent<Collider2D>();
        _selfRenderer = GetComponent<SpriteRenderer>();
        _platformCollider = platformBoundsOverride ?? FindParentCollider();
        _platformRenderer = FindParentRenderer();
        _time = timeOffset;
    }

    private void Update()
    {
        float speed = Mathf.Max(0.0001f, moveSpeed);
        _time += Time.deltaTime * speed;

        Vector2 offset = CalculateOffset(_time);
        Vector3 targetLocal = _startLocalPosition + (Vector3)offset;
        Vector3 targetWorld = transform.parent ? transform.parent.TransformPoint(targetLocal) : targetLocal;

        if (clampToPlatformBounds && TryGetPlatformBounds(out Bounds bounds))
            targetWorld = ClampInsideBounds(targetWorld, bounds);

        transform.position = targetWorld;
    }

    private Vector2 CalculateOffset(float t)
    {
        Vector2 offset = Vector2.zero;

        if (!Mathf.Approximately(localTravel.x, 0f))
        {
            float span = Mathf.Abs(localTravel.x);
            offset.x = Mathf.PingPong(t, span) - span * 0.5f;
        }

        if (!Mathf.Approximately(localTravel.y, 0f))
        {
            float span = Mathf.Abs(localTravel.y);
            offset.y = Mathf.PingPong(t + yAxisPhaseOffset, span) - span * 0.5f;
        }

        return offset;
    }

    private bool TryGetPlatformBounds(out Bounds bounds)
    {
        if (platformBoundsOverride != null)
        {
            bounds = platformBoundsOverride.bounds;
            return true;
        }

        if (_platformCollider != null && _platformCollider != _selfCollider)
        {
            bounds = _platformCollider.bounds;
            return true;
        }

        if (_platformRenderer != null && _platformRenderer != _selfRenderer)
        {
            bounds = _platformRenderer.bounds;
            return true;
        }

        bounds = default;
        return false;
    }

    private Collider2D FindParentCollider()
    {
        Transform current = transform.parent;
        while (current != null)
        {
            Collider2D col = current.GetComponent<Collider2D>();
            if (col != null && col != _selfCollider)
                return col;
            current = current.parent;
        }

        return null;
    }

    private SpriteRenderer FindParentRenderer()
    {
        Transform current = transform.parent;
        while (current != null)
        {
            SpriteRenderer renderer = current.GetComponent<SpriteRenderer>();
            if (renderer != null && renderer != _selfRenderer)
                return renderer;
            current = current.parent;
        }

        return null;
    }

    private Vector3 ClampInsideBounds(Vector3 targetWorld, Bounds bounds)
    {
        Vector2 halfSize = GetObstacleHalfExtents();
        float minX = bounds.min.x + halfSize.x + padding.x;
        float maxX = bounds.max.x - halfSize.x - padding.x;
        float minY = bounds.min.y + halfSize.y + padding.y;
        float maxY = bounds.max.y - halfSize.y - padding.y;

        if (minX > maxX)
        {
            float centerX = (minX + maxX) * 0.5f;
            minX = maxX = centerX;
        }

        if (minY > maxY)
        {
            float centerY = (minY + maxY) * 0.5f;
            minY = maxY = centerY;
        }

        targetWorld.x = Mathf.Clamp(targetWorld.x, minX, maxX);
        targetWorld.y = Mathf.Clamp(targetWorld.y, minY, maxY);
        return targetWorld;
    }

    private Vector2 GetObstacleHalfExtents()
    {
        if (_selfCollider != null)
            return _selfCollider.bounds.extents;

        if (_selfRenderer != null)
            return _selfRenderer.bounds.extents;

        return Vector2.zero;
    }

    /// <summary>
    /// Allows runtime setup of the movement range without touching serialized data.
    /// </summary>
    public void SetLocalTravel(Vector2 travel)
    {
        localTravel = new Vector2(Mathf.Max(0f, travel.x), Mathf.Max(0f, travel.y));
    }
}
