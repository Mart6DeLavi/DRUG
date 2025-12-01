using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    public float amplitude = 2f;
    public float speed = 1.5f;
    public bool randomOffset = true;

    private Vector3 startPos;
    private float offset;

    void Start()
    {
        startPos = transform.position;
        offset = randomOffset ? Random.Range(0f, 10f) : 0f;
    }

    void Update()
    {
        float x = Mathf.Sin((Time.time + offset) * speed) * amplitude;
        transform.position = new Vector3(startPos.x + x, startPos.y, startPos.z);
    }
}
