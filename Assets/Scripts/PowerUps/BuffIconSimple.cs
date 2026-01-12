using UnityEngine;
using UnityEngine.UI;

public class BuffIconSimple : MonoBehaviour
{
    public Image iconImage;

    float duration;
    float timeLeft;
    bool isBlinking;

    public void Init(Sprite sprite, float durationSeconds)
    {
        if (iconImage == null)
            iconImage = GetComponent<Image>();

        iconImage.sprite = sprite;
        duration = durationSeconds;
        timeLeft = durationSeconds;
        isBlinking = false;
    }

    void Update()
    {
        if (duration <= 0f) return;

        timeLeft -= Time.deltaTime;

        // start migania przy 3 sekundach
        if (!isBlinking && timeLeft <= 3f)
            isBlinking = true;

        if (isBlinking)
        {
            // proste miganie alf¹
            float a = (Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f); // 0..1
            var c = iconImage.color;
            c.a = a;
            iconImage.color = c;
        }

        if (timeLeft <= 0f)
            Destroy(gameObject);
    }
}