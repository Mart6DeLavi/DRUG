using UnityEngine;

public class FollowPlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform player;
    public float smoothSpeed = 0.01f;

    private void LateUpdate()
    {
        float lastZ = transform.position.z;
        Vector3 playerPosition = new Vector3(player.position.x, player.position.y, lastZ);
        transform.position = Vector3.Lerp(transform.position, playerPosition, smoothSpeed);
        //transform.position = player.position + new Vector3(player.position.x, player.position.y, lastZ);
    }
}
