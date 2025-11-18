using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSounds : MonoBehaviour
{
    [Header("Jump Sound")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField, Range(0f, 1f)] private float jumpVolume = 1f;
    [SerializeField, Min(0f)] private float jumpStartTime = 0f;

    [Header("Death Sound")]
    [SerializeField] private AudioClip deathClip;
    [SerializeField, Range(0f, 1f)] private float deathVolume = 1f;
    [SerializeField, Min(0f)] private float deathStartTime = 0f;

    private PlayerMovement movement;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void OnEnable()
    {
        if (movement != null)
            movement.OnJump += HandleJump;
    }

    void OnDisable()
    {
        if (movement != null) {
            movement.OnJump -= HandleJump;
        }

    }

    private void HandleJump()
    {
        if (jumpClip != null)
            PlayClipWithOffset(jumpClip, jumpVolume, jumpStartTime);
    }

    public void PlayDeathSound()
    {
        if (deathClip != null)
            PlayClipWithOffset(deathClip, deathVolume, deathStartTime);
    }

    private void PlayClipWithOffset(AudioClip clip, float volume, float startTime)
    {
        GameObject tempGO = new GameObject("TempAudio_" + clip.name);
        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.playOnAwake = false;

        float safeStart = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, clip.length - 0.01f));
        source.time = safeStart;
        source.Play();

        Destroy(tempGO, clip.length - safeStart + 0.1f);
    }
}
