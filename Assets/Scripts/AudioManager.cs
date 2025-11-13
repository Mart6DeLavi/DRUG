using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Range(0f, 1f)]
    public float startVolume = 1f;

    private float lastVolume = 1f;

    private void Awake()
    {
        // prosty singleton, żeby nie było duplikatów między scenami
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetVolume(startVolume);
    }

    // Ustawianie głośności 0–1 (slider)
    public void SetVolume(float value)
    {
        value = Mathf.Clamp01(value);
        AudioListener.volume = value;
        lastVolume = value;
    }

    // Mute/Unmute (np. toggle)
    public void SetMute(bool isMuted)
    {
        if (isMuted)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            AudioListener.volume = lastVolume;
        }
    }

    // Alternatywnie przycisk Mute/Unmute bez boola:
    public void ToggleMute()
    {
        if (AudioListener.volume > 0f)
        {
            lastVolume = AudioListener.volume;
            AudioListener.volume = 0f;
        }
        else
        {
            AudioListener.volume = lastVolume;
        }
    }
}