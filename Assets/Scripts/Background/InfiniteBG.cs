using UnityEngine;

public class InfiniteBackground3 : MonoBehaviour
{
    public Transform bg1;
    public Transform bg2;
    public Transform bg3;
    public Transform player;      // posta� / punkt odniesienia
    public float scrollSpeed = 5f;

    private Transform[] backgrounds;
    private float bgWidth;

    void Start()
    {
        // tablica dla �atwego zarz�dzania kolejk�
        backgrounds = new Transform[3] { bg1, bg2, bg3 };

        // szeroko�� t�a w jednostkach �wiata
        bgWidth = bg1.localScale.x;

        // ustawienie t�a w linii
        backgrounds[0].position = new Vector3(0, backgrounds[0].position.y, backgrounds[0].position.z);
        backgrounds[1].position = new Vector3(backgrounds[0].position.x + bgWidth, backgrounds[1].position.y, backgrounds[1].position.z);
        backgrounds[2].position = new Vector3(backgrounds[1].position.x + bgWidth, backgrounds[2].position.y, backgrounds[2].position.z);
    }

    void Update()
    {
        // przesuwanie t�a w lewo
        for (int i = 0; i < 3; i++)
        {
            backgrounds[i].position += Vector3.left * scrollSpeed * Time.deltaTime;
        }

        // sprawdzamy, czy pierwszy w kolejce t�o wysz�o poza kamer� (np. posta�)
        if (player.position.x > backgrounds[0].position.x + bgWidth)
        {
            // przenosimy pierwsze t�o za ostatnie
            float newX = backgrounds[2].position.x + bgWidth;
            backgrounds[0].position = new Vector3(newX, backgrounds[0].position.y, backgrounds[0].position.z);

            // shift queue in array
            Transform temp = backgrounds[0];
            backgrounds[0] = backgrounds[1];
            backgrounds[1] = backgrounds[2];
            backgrounds[2] = temp;
        }
    }
}
