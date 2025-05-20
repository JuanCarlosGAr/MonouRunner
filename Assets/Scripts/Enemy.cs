using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public Transform player; // Referencia al jugador
    public float speed = 5f; // Velocidad de movimiento del enemigo
    public GameObject gameOverPanel; // Referencia al panel de Game Over
    public TMPro.TextMeshProUGUI distanceText; // Referencia al texto de distancia
    public TMPro.TextMeshProUGUI coinText; // Referencia al texto de monedas

    private bool isGameOver = false; // Bandera para controlar el estado del juego
    private bool canMove = false; // Bandera para controlar si el enemigo puede moverse

    private void Start()
    {
        // Iniciar la corrutina para esperar 5 segundos antes de moverse
        StartCoroutine(WaitBeforeMoving());
    }

    private void Update()
    {
        if (isGameOver || !canMove) return; // Si el juego terminó o no puede moverse, no hacer nada

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

    private IEnumerator WaitBeforeMoving()
    {
        // Esperar 5 segundos
        yield return new WaitForSeconds(3f);
        // Permitir que el enemigo se mueva
        canMove = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Detener el tiempo del juego
            Time.timeScale = 0;

            // Establecer el estado de Game Over
            isGameOver = true;

            // Mostrar el panel de Game Over
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                print("Game Over");
            }

            // Actualizar los textos de distancia y monedas
            if (distanceText != null && coinText != null)
            {
                ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
                if (scoreManager != null)
                {
                    distanceText.text = "Distancia: " + Mathf.FloorToInt(scoreManager.GetDistance()) + " m";
                    coinText.text = "Monedas: " + scoreManager.GetCoins();

                    try
                    {
                        int coinCount = int.Parse(coinText.text.Replace("Monedas: ", "").Trim());
                        Monou.MonouArcadeManager.inst.Success(coinCount);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Error calling MonouArcadeManager.Success: " + ex.Message);
                    }
                }
            }
        }
    }
}
