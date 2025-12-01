using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int coinValue = 1; // ile monet daje ten obiekt

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // upewnij siê, ¿e gracz ma tag "Player"
        {
            CurrencyManager.Instance.AddCoins(coinValue);

            // (opcjonalnie) animacja, dŸwiêk, efekt
            Destroy(gameObject); // usuwa monetê po zebraniu
        }
    }
}