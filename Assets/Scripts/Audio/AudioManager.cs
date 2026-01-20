using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    // ===================== PLAYER PREF KEYS =====================
    private const string PREF_MUSIC_VOL  = "music_volume_slider"; // float 0..1
    private const string PREF_MUSIC_MUTE = "music_muted";         // int 0/1

    [Header("Music Volume (Global)")]
    [Tooltip("Default music slider value if player has no saved settings yet.")]
    [Range(0f, 1f)] public float startMusicVolume = 1f;

    [Tooltip("If true, music will start muted when no saved settings exist.")]
    public bool startMuted = false;

    [Tooltip("If false, mute will NOT be saved between game restarts (recommended for a simple OnClick button).")]
    public bool persistMuteToPrefs = false;

    [Tooltip("If true, slider uses a perceptual curve (more natural loudness response).")]
    public bool usePerceptualCurve = true;

    [Tooltip("Exponent used when usePerceptualCurve is enabled. 2 is a good default.")]
    public float perceptualExponent = 2f;

    [Tooltip("Minimum audible volume for the slider. Keeps the slider from instantly muting when UI briefly sends 0.")]
    [Range(0f, 1f)] public float sliderMinAudible = 0.01f;

    [Tooltip("If true, moving the slider above the minimum will automatically unmute music.")]
    public bool sliderAutoUnmute = true;

    // Runtime music settings (persisted)
    private float _musicSlider = 1f;  // 0..1
    private bool _musicMuted = false;

    // Last non-zero slider value before mute (helps if UI sets slider to 0 when muting)
    private float _musicBeforeMute = 1f;

    // Current fade multiplier (0..1). Final volume = fade01 * (muted ? 0 : slider)
    private float _fade01 = 1f;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip buttonClickClip;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;

    [Header("Legacy Music Clips (optional)")]
    [Tooltip("Optional: used by old code calling PlayMenuMusic(). If empty, AudioManager will try Scene Music Table entry for the current scene.")]
    [SerializeField] private AudioClip menuMusic;

    [Tooltip("Optional: used by old code calling PlayGameMusic(). If empty, AudioManager will try Scene Music Table entry for the current scene.")]
    [SerializeField] private AudioClip gameMusic;

    [Header("Bonus SFX")]
    [SerializeField] private AudioClip buffGoodClip;

    [Header("Debuff SFX")]
    [SerializeField] private AudioClip debuffClip;

    // -----------------------------
    // Scene -> Music "table"
    // -----------------------------
    public enum PitchControlMode
    {
        None,
        GameSpeed01,
        TimeInScene01,
        GameSpeedOrTime01
    }

    [Serializable]
    public class SceneMusicEntry
    {
        [Tooltip("Exact scene name (as in Build Settings).")]
        public string sceneName;

        [Tooltip("Music clip to play in this scene.")]
        public AudioClip musicClip;

        public bool loop = true;

        [Header("Transition")]
        [Tooltip("Fade-out + switch + fade-in duration (seconds).")]
        public float fadeTime = 0.35f;

        [Header("Pitch / Tempo")]
        public PitchControlMode pitchControl = PitchControlMode.None;

        [Tooltip("Pitch when normalized = 0.")]
        public float minPitch = 1.0f;

        [Tooltip("Pitch when normalized = 1.")]
        public float maxPitch = 1.3f;

        [Tooltip("How fast pitch approaches target (bigger = faster).")]
        public float pitchSmooth = 6f;

        [Header("Time-based ramp")]
        [Tooltip("Seconds to reach normalized=1 using time-in-scene ramp. 0 disables time ramp.")]
        public float secondsToMaxTempo = 0f;
    }

    [Header("Scene Music Table")]
    [SerializeField] private List<SceneMusicEntry> sceneMusic = new List<SceneMusicEntry>();

    [Header("Fallback (if scene not in table)")]
    [SerializeField] private AudioClip fallbackMusic;
    [SerializeField] private float fallbackFadeTime = 0.35f;

    // Runtime
    private SceneMusicEntry _activeEntry;
    private float _sceneEnterTime;
    private float _gameSpeed01;
    private float _targetPitch = 1f;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        // Simple singleton to avoid duplicates between scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadMusicSettings();
        ApplyMusicVolumeNow();

        if (musicSource != null)
            musicSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void Update()
    {
        if (musicSource == null)
            return;

        // Always enforce volume each frame (fixes cases where other scripts/fades overwrite AudioSource.volume)
        ApplyMusicVolumeNow();

        if (!musicSource.isPlaying)
            return;

        float smooth = (_activeEntry != null) ? Mathf.Max(0.01f, _activeEntry.pitchSmooth) : 6f;

        // Smooth pitch
        float current = musicSource.pitch;
        float next = Mathf.Lerp(current, _targetPitch, 1f - Mathf.Exp(-smooth * Time.unscaledDeltaTime));
        musicSource.pitch = next;

        // If scene uses time ramp, keep recomputing target as time passes
        if (_activeEntry != null &&
            (_activeEntry.pitchControl == PitchControlMode.TimeInScene01 ||
             _activeEntry.pitchControl == PitchControlMode.GameSpeedOrTime01) &&
            _activeEntry.secondsToMaxTempo > 0.001f)
        {
            RecomputeTargetPitch();
        }
    }

    // ===================== GLOBAL MUSIC VOLUME + MUTE =====================

    public void SetMusicVolumeSlider(float value01)
    {
        // Some UI setups can momentarily send 0 when clicking the slider.
        // We clamp to a small minimum so the music doesn't "hard mute" unless the Mute button is used.
        float min = Mathf.Clamp01(sliderMinAudible);
        value01 = Mathf.Clamp01(value01);
        if (value01 <= 0.0001f)
            value01 = min;
        else if (value01 < min)
            value01 = min;

        _musicSlider = value01;
        PlayerPrefs.SetFloat(PREF_MUSIC_VOL, _musicSlider);

        // Track last meaningful volume so unmute can restore it if needed
        if (_musicSlider > 0.0001f)
            _musicBeforeMute = _musicSlider;

        // Optional: if player moves slider up, auto-unmute
        if (sliderAutoUnmute && _musicMuted && _musicSlider > min + 0.0001f)
            _musicMuted = false;

        ApplyMusicVolumeNow();
        EnsureMusicPlayingIfNeeded();
    }

    public void SetMusicMute(bool isMuted)
    {
        if (isMuted && _musicSlider > 0.0001f)
            _musicBeforeMute = _musicSlider;

        _musicMuted = isMuted;

        // If UI moved slider to 0 while muting, restore last meaningful volume on unmute
        if (!_musicMuted && _musicSlider <= 0.0001f)
        {
            _musicSlider = Mathf.Clamp01(_musicBeforeMute <= 0.0001f ? startMusicVolume : _musicBeforeMute);
            PlayerPrefs.SetFloat(PREF_MUSIC_VOL, _musicSlider);
        }

        if (persistMuteToPrefs)
            PlayerPrefs.SetInt(PREF_MUSIC_MUTE, _musicMuted ? 1 : 0);

        ApplyMusicVolumeNow();
        EnsureMusicPlayingIfNeeded();
    }

    public void ToggleMusicMute()
    {
        SetMusicMute(!_musicMuted);
    }

    public void OnMusicMuteButtonClicked()
    {
        ToggleMusicMute();
    }

    public float GetMusicVolumeSlider() => _musicSlider;
    public bool IsMusicMuted() => _musicMuted;

    private float GetEffectiveMusicVolume01()
    {
        if (_musicMuted) return 0f;

        float v = Mathf.Clamp01(_musicSlider);
        if (!usePerceptualCurve) return v;

        float exp = Mathf.Max(0.01f, perceptualExponent);
        return Mathf.Pow(v, exp);
    }

    private void ApplyMusicVolumeNow()
    {
        if (musicSource == null) return;

        float effective = GetEffectiveMusicVolume01();
        musicSource.volume = Mathf.Clamp01(_fade01) * Mathf.Clamp01(effective);
    }

    private void EnsureMusicPlayingIfNeeded()
    {
        if (musicSource == null) return;
        if (_musicMuted) return;
        if (musicSource.clip == null) return;

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    private void LoadMusicSettings()
    {
        if (!PlayerPrefs.HasKey(PREF_MUSIC_VOL))
            PlayerPrefs.SetFloat(PREF_MUSIC_VOL, startMusicVolume);

        _musicSlider = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MUSIC_VOL, startMusicVolume));

        // Prevent loading a "stuck at 0" slider value that effectively mutes the game.
        float min = Mathf.Clamp01(sliderMinAudible);
        if (_musicSlider <= 0.0001f)
            _musicSlider = min;
        else if (_musicSlider < min)
            _musicSlider = min;

        PlayerPrefs.SetFloat(PREF_MUSIC_VOL, _musicSlider);

        if (_musicSlider > 0.0001f)
            _musicBeforeMute = _musicSlider;
        else
            _musicBeforeMute = Mathf.Clamp01(startMusicVolume);

        if (persistMuteToPrefs)
        {
            if (!PlayerPrefs.HasKey(PREF_MUSIC_MUTE))
                PlayerPrefs.SetInt(PREF_MUSIC_MUTE, startMuted ? 1 : 0);

            _musicMuted = PlayerPrefs.GetInt(PREF_MUSIC_MUTE, startMuted ? 1 : 0) == 1;
        }
        else
        {
            _musicMuted = startMuted;
        }

        _fade01 = 1f;
    }

    // ============= SFX (clicks) =============

    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayButtonClick() => PlaySfx(buttonClickClip);
    public void PlayBuffGood() => PlaySfx(buffGoodClip);
    public void PlayDebuff() => PlaySfx(debuffClip);

    // ===================== SCENE MUSIC =====================

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _sceneEnterTime = Time.unscaledTime;
        _activeEntry = FindEntry(scene.name);

        AudioClip clipToPlay = _activeEntry != null ? _activeEntry.musicClip : fallbackMusic;
        float fade = _activeEntry != null ? _activeEntry.fadeTime : fallbackFadeTime;

        if (musicSource == null)
            return;

        if (clipToPlay == null)
        {
            StopMusicImmediate();
            return;
        }

        // Don't restart the same clip
        if (musicSource.isPlaying && musicSource.clip == clipToPlay)
        {
            ApplyActiveEntrySettings();
            RecomputeTargetPitch();
            ApplyMusicVolumeNow();
            EnsureMusicPlayingIfNeeded();
            return;
        }

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeToClip(clipToPlay, fade));
    }

    private SceneMusicEntry FindEntry(string sceneName)
    {
        for (int i = 0; i < sceneMusic.Count; i++)
        {
            var e = sceneMusic[i];
            if (e != null && !string.IsNullOrWhiteSpace(e.sceneName) && e.sceneName == sceneName)
                return e;
        }
        return null;
    }

    private void ApplyActiveEntrySettings()
    {
        if (musicSource == null) return;
        musicSource.loop = _activeEntry != null ? _activeEntry.loop : true;
    }

    private System.Collections.IEnumerator FadeToClip(AudioClip newClip, float fadeTime)
    {
        if (musicSource == null) yield break;

        float t = 0f;
        float startFade = _fade01;

        // Fade out
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float a = fadeTime <= 0.0001f ? 1f : (t / fadeTime);
            float fade01 = Mathf.Lerp(startFade, 0f, a);
            SetMusicFade01(fade01);
            yield return null;
        }

        // Switch clip
        musicSource.clip = newClip;
        ApplyActiveEntrySettings();
        musicSource.Play();

        // Reset pitch for new scene
        RecomputeTargetPitch();
        musicSource.pitch = Mathf.Clamp(_targetPitch, 0.1f, 3f);

        // Fade in
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float a = fadeTime <= 0.0001f ? 1f : (t / fadeTime);
            float fade01 = Mathf.Lerp(0f, 1f, a);
            SetMusicFade01(fade01);
            yield return null;
        }

        SetMusicFade01(1f);
        ApplyMusicVolumeNow();
        EnsureMusicPlayingIfNeeded();
        _fadeRoutine = null;
    }

    private void SetMusicFade01(float fade01)
    {
        if (musicSource == null) return;
        _fade01 = Mathf.Clamp01(fade01);
        ApplyMusicVolumeNow();
    }

    private void StopMusicImmediate()
    {
        if (musicSource == null) return;
        musicSource.Stop();
        musicSource.clip = null;

        _fade01 = 1f;
        ApplyMusicVolumeNow();
    }

    // ============= GAME SPEED -> MUSIC TEMPO =============

    public void SetGameSpeed01(float normalizedSpeed)
    {
        _gameSpeed01 = Mathf.Clamp01(normalizedSpeed);
        RecomputeTargetPitch();
    }

    public void SetMusicPitch(float pitch)
    {
        if (musicSource == null) return;

        float min = (_activeEntry != null) ? _activeEntry.minPitch : 0.5f;
        float max = (_activeEntry != null) ? _activeEntry.maxPitch : 3f;

        _targetPitch = Mathf.Clamp(pitch, min, max);
    }

    private void RecomputeTargetPitch()
    {
        if (musicSource == null) return;

        if (_activeEntry == null)
        {
            _targetPitch = 1f;
            return;
        }

        float time01 = 0f;
        if (_activeEntry.secondsToMaxTempo > 0.001f)
        {
            float elapsed = Time.unscaledTime - _sceneEnterTime;
            time01 = Mathf.Clamp01(elapsed / _activeEntry.secondsToMaxTempo);
        }

        float normalized = 0f;

        switch (_activeEntry.pitchControl)
        {
            case PitchControlMode.None:
                _targetPitch = 1f;
                return;

            case PitchControlMode.GameSpeed01:
                normalized = _gameSpeed01;
                break;

            case PitchControlMode.TimeInScene01:
                normalized = time01;
                break;

            case PitchControlMode.GameSpeedOrTime01:
                normalized = Mathf.Max(_gameSpeed01, time01);
                break;
        }

        normalized = Mathf.Clamp01(normalized);

        float minP = Mathf.Clamp(_activeEntry.minPitch, 0.1f, 3f);
        float maxP = Mathf.Clamp(_activeEntry.maxPitch, 0.1f, 3f);
        if (maxP < minP) maxP = minP;

        _targetPitch = Mathf.Lerp(minP, maxP, normalized);
    }

    // ===================== LEGACY API (compatibility) =====================

    public void PlayMenuMusic()
    {
        if (menuMusic != null)
        {
            PlayClip(menuMusic, fallbackFadeTime);
            return;
        }

        PlaySceneMusic(SceneManager.GetActiveScene().name);
    }

    public void PlayGameMusic()
    {
        if (gameMusic != null)
        {
            PlayClip(gameMusic, fallbackFadeTime);
            return;
        }

        PlaySceneMusic(SceneManager.GetActiveScene().name);
    }

    public void PlaySceneMusic(string sceneName)
    {
        if (musicSource == null) return;

        _sceneEnterTime = Time.unscaledTime;
        _activeEntry = FindEntry(sceneName);

        AudioClip clipToPlay = _activeEntry != null ? _activeEntry.musicClip : fallbackMusic;
        float fade = _activeEntry != null ? _activeEntry.fadeTime : fallbackFadeTime;

        if (clipToPlay == null)
        {
            StopMusicImmediate();
            return;
        }

        if (musicSource.isPlaying && musicSource.clip == clipToPlay)
        {
            ApplyActiveEntrySettings();
            RecomputeTargetPitch();
            ApplyMusicVolumeNow();
            EnsureMusicPlayingIfNeeded();
            return;
        }

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeToClip(clipToPlay, fade));
    }

    public void PlayClip(AudioClip clip, float fadeTime = 0.35f, bool loop = true)
    {
        if (musicSource == null || clip == null) return;

        if (musicSource.isPlaying && musicSource.clip == clip)
            return;

        musicSource.loop = loop;

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeToClip(clip, fadeTime));
    }
}