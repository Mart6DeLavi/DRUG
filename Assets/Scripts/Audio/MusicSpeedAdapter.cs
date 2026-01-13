using UnityEngine;

public class MusicSpeedAdapter : MonoBehaviour
{
    private float last = -1f;

    void Update()
    {
        // If something is missing - do nothing
        if (AudioManager.Instance == null || GameSpeedController.Instance == null)
            return;

        // Current game acceleration
        float current = GameSpeedController.Instance.CurrentMultiplier;

        // Min and max multiplier from GameSpeedController
        float min = GameSpeedController.Instance.startMultiplier;
        float max = GameSpeedController.Instance.maxMultiplier;

        // Normalize to 0..1 range
        float normalized = Mathf.InverseLerp(min, max, current);

        // Pass to AudioManager - it will handle pitch between minMusicPitch and maxMusicPitch
        AudioManager.Instance.SetGameSpeed01(normalized);
    }
}