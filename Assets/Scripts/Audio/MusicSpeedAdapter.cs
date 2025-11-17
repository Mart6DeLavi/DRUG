using UnityEngine;

public class MusicSpeedAdapter : MonoBehaviour
{
    void Update()
    {
        // Если кого-то нет – ничего не делаем
        if (AudioManager.Instance == null || GameSpeedController.Instance == null)
            return;

        // Текущее ускорение игры
        float current = GameSpeedController.Instance.CurrentMultiplier;

        // Минимальный и максимальный множитель из GameSpeedController
        float min = GameSpeedController.Instance.startMultiplier;
        float max = GameSpeedController.Instance.maxMultiplier;

        // Нормализуем в диапазон 0..1
        float normalized = Mathf.InverseLerp(min, max, current);

        // Отдаём в AudioManager – он уже сам сделает pitch między minMusicPitch i maxMusicPitch
        AudioManager.Instance.SetGameSpeed01(normalized);
    }
}