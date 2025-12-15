using UnityEngine;

public class MusicSpeedAdapter : MonoBehaviour
{
    private float last = -1f;

    void Update()
    {
        if (AudioManager.Instance == null || GameSpeedController.Instance == null)
            return;

        float current = GameSpeedController.Instance.CurrentMultiplier;
        float min = GameSpeedController.Instance.startMultiplier;
        float max = GameSpeedController.Instance.maxMultiplier;

        float normalized = Mathf.InverseLerp(min, max, current);

        if (!Mathf.Approximately(normalized, last))
        {
            AudioManager.Instance.SetGameSpeed01(normalized);
            last = normalized;
        }
    }
}