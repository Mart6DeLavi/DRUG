using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        // simple singleton to avoid duplicates between scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetVolume(startVolume);

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
        if (musicSource == null || !musicSource.isPlaying)
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

    // ===================== GLOBAL VOLUME =====================

    public void SetVolume(float value)
    {
        value = Mathf.Clamp01(value);
        AudioListener.volume = value;
        lastVolume = value;
    }

    public void SetMute(bool isMuted)
    {
        AudioListener.volume = isMuted ? 0f : lastVolume;
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
        float startVol = musicSource.volume;

        // Fade out
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float a = fadeTime <= 0.0001f ? 1f : (t / fadeTime);
            musicSource.volume = Mathf.Lerp(startVol, 0f, a);
            yield return null;
        }

        // Switch
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
            musicSource.volume = Mathf.Lerp(0f, startVol, a);
            yield return null;
        }

        musicSource.volume = startVol;
        _fadeRoutine = null;
    }

    private void StopMusicImmediate()
    {
        if (musicSource == null) return;
        musicSource.Stop();
        musicSource.clip = null;
    }

        // ============= GAME SPEED -> MUSIC TEMPO =============

    /// <summary>
    /// normalizedSpeed - value 0..1
    /// 0  -> minMusicPitch
    /// 1  -> maxMusicPitch
    /// Call this where you update game speed.
    /// </summary>
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
                // In "None" mode we want the clip to play at normal speed regardless of min/max inspector values.
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

        // Safety: prevent accidental 0 pitch (silent) from inspector misconfiguration
        float minP = Mathf.Clamp(_activeEntry.minPitch, 0.1f, 3f);
        float maxP = Mathf.Clamp(_activeEntry.maxPitch, 0.1f, 3f);
        if (maxP < minP) maxP = minP;

        _targetPitch = Mathf.Lerp(minP, maxP, normalized);
    }
    // ===================== LEGACY API (compatibility) =====================
    // Some older scripts may still call these. They now route into the Scene Music system.

    public void PlayMenuMusic()
    {
        // Prefer legacy clip if provided
        if (menuMusic != null)
        {
            PlayClip(menuMusic, fallbackFadeTime);
            return;
        }

        // Otherwise, try to play whatever is configured for the current scene
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

    /// <summary>
    /// Manually trigger music for a specific scene name (must match Build Settings / Scene Music Table).
    /// Useful if you want to play "Menu" music while staying in the same scene, etc.
    /// </summary>
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

        // Avoid restarting same clip
        if (musicSource.isPlaying && musicSource.clip == clipToPlay)
        {
            ApplyActiveEntrySettings();
            RecomputeTargetPitch();
            return;
        }

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeToClip(clipToPlay, fade));
    }

    /// <summary>
    /// Manually play a specific music clip (bypasses Scene Music Table). Keeps current entry settings.
    /// </summary>
    public void PlayClip(AudioClip clip, float fadeTime = 0.35f, bool loop = true)
    {
        if (musicSource == null || clip == null) return;

        // When playing an explicit clip, do not change active entry; just play the clip
        if (musicSource.isPlaying && musicSource.clip == clip)
            return;

        musicSource.loop = loop;

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeToClip(clip, fadeTime));
    }
}