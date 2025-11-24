using System.Collections;
using UnityEngine;

/// <summary>
/// Makes tiles fade out and disappear after the player steps on them.
/// Can affect only this tile or the whole platform segment.
/// Optionally ignores trigger tiles (e.g. lava).
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class DisappearingTile : MonoBehaviour
{
    [Header("Disappear timing")]
    [Tooltip("Delay before the tile starts disappearing after the player steps on it.")]
    public float delayBeforeDisappear = 0.5f;

    [Tooltip("How long the fade-out animation lasts (seconds).")]
    public float fadeDuration = 0.3f;

    [Tooltip("Delay before the tile reappears (<= 0 = never reappear).")]
    public float delayBeforeReappear = 0f;

    [Header("Behaviour")]
    [Tooltip("If true, disappears the whole platform segment instead of only this tile.")]
    public bool affectWholeSegment = true;

    [Tooltip("If true, tiles with isTrigger=true (e.g. lava) are NOT affected.")]
    public bool ignoreTriggerTiles = true;

    private bool triggered = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (triggered) return;
        if (!collision.collider.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(DisappearRoutine());
    }

    // Jeśli kiedyś zechcesz używać triggerów zamiast kolizji, możesz odkomentować:
//  void OnTriggerEnter2D(Collider2D other)
//  {
//      if (triggered) return;
//      if (!other.CompareTag("Player")) return;
//
//      triggered = true;
//      StartCoroutine(DisappearRoutine());
//  }

    private IEnumerator DisappearRoutine()
    {
        // Czekamy przed startem zanikania
        if (delayBeforeDisappear > 0f)
            yield return new WaitForSeconds(delayBeforeDisappear);

        if (affectWholeSegment && transform.parent != null)
        {
            // DZIAŁAMY NA CAŁEJ PLATFORMIE (segment)
            SpriteRenderer[] sprites = transform.parent.GetComponentsInChildren<SpriteRenderer>();
            Collider2D[] colliders = transform.parent.GetComponentsInChildren<Collider2D>();

            // Fade OUT
            yield return Fade(sprites, 1f, 0f);

            // Wyłączamy collidery (pomijając triggery, jeśli tak ustawione)
            foreach (Collider2D col in colliders)
            {
                if (ignoreTriggerTiles && col.isTrigger) continue;
                col.enabled = false;
            }
        }
        else
        {
            // TYLKO TEN JEDEN TILE
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Collider2D col = GetComponent<Collider2D>();

            yield return Fade(new SpriteRenderer[] { sr }, 1f, 0f);

            if (!(ignoreTriggerTiles && col != null && col.isTrigger))
                if (col != null) col.enabled = false;
        }

        // Nie wracamy – koniec
        if (delayBeforeReappear <= 0f)
            yield break;

        // Czekamy przed powrotem
        yield return new WaitForSeconds(delayBeforeReappear);

        if (affectWholeSegment && transform.parent != null)
        {
            SpriteRenderer[] sprites = transform.parent.GetComponentsInChildren<SpriteRenderer>();
            Collider2D[] colliders = transform.parent.GetComponentsInChildren<Collider2D>();

            // Włączamy collidery z powrotem
            foreach (Collider2D col in colliders)
            {
                if (ignoreTriggerTiles && col.isTrigger) continue;
                col.enabled = true;
            }

            // Fade IN
            yield return Fade(sprites, 0f, 1f);
        }
        else
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Collider2D col = GetComponent<Collider2D>();

            if (!(ignoreTriggerTiles && col != null && col.isTrigger))
                if (col != null) col.enabled = true;

            yield return Fade(new SpriteRenderer[] { sr }, 0f, 1f);
        }

        triggered = false;
    }

    private IEnumerator Fade(SpriteRenderer[] sprites, float from, float to)
    {
        float time = 0f;
        float duration = Mathf.Max(0.0001f, fadeDuration); // żeby nie dzielić przez zero

        while (time < duration)
        {
            float alpha = Mathf.Lerp(from, to, time / duration);

            foreach (SpriteRenderer sr in sprites)
            {
                if (!sr) continue;

                if (ignoreTriggerTiles)
                {
                    Collider2D col = sr.GetComponent<Collider2D>();
                    if (col != null && col.isTrigger)
                        continue; // pomijamy np. lawę
                }

                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }

            time += Time.deltaTime;
            yield return null;
        }

        // ustawiamy końcową alphę „na sztywno”
        foreach (SpriteRenderer sr in sprites)
        {
            if (!sr) continue;

            if (ignoreTriggerTiles)
            {
                Collider2D col = sr.GetComponent<Collider2D>();
                if (col != null && col.isTrigger)
                    continue;
            }

            Color c = sr.color;
            c.a = to;
            sr.color = c;
        }
    }
}