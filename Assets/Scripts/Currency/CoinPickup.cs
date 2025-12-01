using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int coinValue = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CurrencyManager.Instance.AddCoins(coinValue);

            
            Destroy(gameObject);
        }
    }
}