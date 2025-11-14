using UnityEngine;

// Simple helper attached to each generated platform segment.
// Used to track start/end positions for cleanup.
public class PlatformSegmentMarker : MonoBehaviour
{
    public float startX; // World X position of the first tile in this segment
    public float endX;   // World X position AFTER the last tile in this segment (exclusive)
}
