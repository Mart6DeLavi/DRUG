using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSounds : MonoBehaviour
{
    [Header("Jump Sound")]
    [SerializeField] private AudioClip jumpClip;                      // Sound to play on jump
    [SerializeField, Range(0f, 1f)] private float jumpVolume = 1f;    // Jump volume
    [SerializeField, Min(0f)] private float jumpStartTime = 0f;       // Seconds to skip from clip start

    [Header("Death Sound")]
    [SerializeField] private AudioClip deathClip;                     // Sound to play on death
    [SerializeField, Range(0f, 1f)] private float deathVolume = 1f;   // Death volume
    [SerializeField, Min(0f)] private float deathStartTime = 0f;      // Seconds to skip from clip start

    private PlayerMovement movement;          // Movement script to listen to
    private bool playedDeathSound = false;    // Prevent double play on disable

    void Awake()
    {
        // Get reference to PlayerMovement on the same GameObject
        movement = GetComponent<PlayerMovement>();
    }

    void OnEnable()
    {
        // Subscribe to jump event (if movement exists)
        if (movement != null)
            movement.OnJump += HandleJump;
    }

    void OnDisable()
    {
        // Unsubscribe from jump event
        if (movement != null)
            movement.OnJump -= HandleJump;

        // Play death sound when object gets disabled/destroyed (e.g., scene reload)
        if (!playedDeathSound && deathClip != null)
        {
            PlayClipWithOffset(deathClip, deathVolume, deathStartTime);
            playedDeathSound = true;
        }
    }

    // Called when PlayerMovement fires OnJump
    private void HandleJump()
    {
        if (jumpClip != null)
            PlayClipWithOffset(jumpClip, jumpVolume, jumpStartTime);
    }

    // Play an AudioClip starting at a given time offset (seconds)
    private void PlayClipWithOffset(AudioClip clip, float volume, float startTime)
    {
        GameObject tempGO = new GameObject("TempAudio_" + clip.name);
        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.playOnAwake = false;

        // Clamp startTime within clip length
        float safeStart = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, clip.length - 0.01f));
        source.time = safeStart;
        source.Play();

        // Auto-cleanup after playback
        Destroy(tempGO, clip.length - safeStart + 0.1f);
    }
}