using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Range(0f, 1f)]
    public float startVolume = 1f;
    private float lastVolume = 1f;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip buttonClickClip;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;  
    [SerializeField] private AudioClip menuMusic;       
    [SerializeField] private AudioClip gameMusic;   

        [Header("Music Speed / Pitch")]
    [SerializeField] private float minMusicPitch = 1.0f;  // normalne tempo
    [SerializeField] private float maxMusicPitch = 1.3f;  // tempo przy maksymalnej prędkości gry    

    [Header("Bonus SFX")]
    [SerializeField] private AudioClip buffGoodClip;   // dobry buff
    
    [Header("Debuff SFX")]
    [SerializeField] private AudioClip debuffClip;

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

    // ============= GŁOŚNOŚĆ GLOBALNA =============

    public void SetVolume(float value)
    {
        value = Mathf.Clamp01(value);
        AudioListener.volume = value;
        lastVolume = value;
    }

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

    // ============= SFX (kliknięcia) =============

    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayButtonClick()
    {
        PlaySfx(buttonClickClip);
    }

    public void PlayBuffGood()
    {
        PlaySfx(buffGoodClip);
    }
    
    public void PlayDebuff()
    {
        PlaySfx(debuffClip);
    }

    // ============= MUZYKA =============

    public void PlayMenuMusic()
    {
        if (musicSource == null || menuMusic == null) return;

        musicSource.clip = menuMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayGameMusic()
    {
        if (musicSource == null || gameMusic == null) return;

        musicSource.clip = gameMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }

        // ============= PRĘDKOŚĆ GRY -> TEMPO MUZYKI =============

    /// <summary>
    /// normalizedSpeed – wartość 0..1
    /// 0  -> minMusicPitch
    /// 1  -> maxMusicPitch
    /// Wołaj to z miejsca, w którym aktualizujesz prędkość gry.
    /// </summary>
    public void SetGameSpeed01(float normalizedSpeed)
    {
        if (musicSource == null) return;

        float t = Mathf.Clamp01(normalizedSpeed);
        float targetPitch = Mathf.Lerp(minMusicPitch, maxMusicPitch, t);
        musicSource.pitch = targetPitch;
    }

    public void SetMusicPitch(float pitch)
    {
        if (musicSource == null) return;

        // Bezpieczne ograniczenie
        float clamped = Mathf.Clamp(pitch, minMusicPitch, maxMusicPitch);
        musicSource.pitch = clamped;
    }
    
}