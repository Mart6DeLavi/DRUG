using UnityEngine;

public class TiledBackgroundScroll : MonoBehaviour
{
    public float scrollSpeed = 2f; // prędkość przesuwania
    private SpriteRenderer sr;
    private float spriteWidth;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        // Szerokość sprite w jednostkach świata (Width * scale)
        spriteWidth = sr.size.x;
    }

    void Update()
    {
        // Przesuwanie obiektu w lewo
        transform.position += Vector3.left * scrollSpeed * Time.deltaTime;

        // Kiedy obiekt przesunie się całkowicie w lewo, resetujemy pozycję
        if (transform.position.x <= -spriteWidth)
        {
            transform.position += Vector3.right * spriteWidth;
        }
    }
}
