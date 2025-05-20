using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player; // Referencia al jugador
    public float speed = 5f; // Velocidad de movimiento del enemigo
    public GameObject gameOverPanel; // Referencia al panel de Game Over
    public TMPro.TextMeshProUGUI distanceText; // Referencia al texto de distancia
    public TMPro.TextMeshProUGUI coinText; // Referencia al texto de monedas

    private void Update()
    {
        if (player != null)
        {
            // Calcular la distancia al jugador
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            Debug.Log("Distancia al jugador: " + distanceToPlayer);

            // Mover al enemigo hacia el jugador
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Detener el tiempo del juego
            Time.timeScale = 0;

            // Mostrar el panel de Game Over
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            // Actualizar los textos de distancia y monedas
            if (distanceText != null && coinText != null)
            {
                ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
                if (scoreManager != null)
                {
                    distanceText.text = "Distancia: " + Mathf.FloorToInt(scoreManager.GetDistance()) + " m";
                    coinText.text = "Monedas: " + scoreManager.GetCoins();
                }
            }
        }
    }
}
