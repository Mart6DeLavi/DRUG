using UnityEngine;

public class PickupEffect : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip effectSound;

    [Header("Effect type")]
    public bool isBuff = true;
    public string effectType = "SpeedBoost";

    AudioSource audioSrc;

    private void Awake()
    {
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null)
            audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
           ;

            // D�wi�k BEZPIECZNY - globalny lub op�niony
            if (effectSound != null)
            {
                // Opcja 1: Globalny d�wi�k (najpewniejszy)
                AudioSource.PlayClipAtPoint(effectSound, Camera.main.transform.position, 0.8f);
            }
            else

            Destroy(gameObject, 0.5f);
        }
    }

}
