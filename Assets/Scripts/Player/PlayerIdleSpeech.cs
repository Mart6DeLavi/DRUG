using UnityEngine;
using TMPro;

public class PlayerIdleSpeech : MonoBehaviour
{
    public TextMeshProUGUI speechText;   // Text inside the speech bubble
    public GameObject speechBubble;       // Whole bubble (Image + Text)
    public float timeToShow = 4.5f;

    private Vector3 lastPosition;
    private float idleTimer;

    private string[] messages = {
        "Przynajmniej zabije mnie czas, a nie twoje umiejętności",
        "Dobrze Ci idzie:) Nic nie robienie...",
        "To byłby dobry moment, żeby ruszyć.",
        "Ściana nie będzie czekać.",
        "Nie śpiesz się... i tak prędzej czy później umrę",
        "Ja bym poszedł dalej... ale co ja tam wiem",
        "Może... strzałka w prawo? Tak tylko sugeruję."
    };

    void Start()
    {
        lastPosition = transform.position;
        speechBubble.SetActive(false);       // Hide the bubble on start
        speechText.gameObject.SetActive(false); // Hide the text as well
    }

    void Update()
    {
        // Check whether the player is moving
        if (Vector3.Distance(transform.position, lastPosition) < 0.001f)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= timeToShow && !speechBubble.activeSelf)
            {
                ShowRandomMessage();
            }
        }
        else
        {
            idleTimer = 0f;
            // Hide the whole bubble including text
            speechBubble.SetActive(false);
            speechText.gameObject.SetActive(false);
        }

        lastPosition = transform.position;
    }

    void ShowRandomMessage()
    {
        int index = Random.Range(0, messages.Length);
        speechText.text = messages[index];   // Set random message text
        speechBubble.SetActive(true);        // Enable bubble image
        speechText.gameObject.SetActive(true); // Enable bubble text
    }
}
