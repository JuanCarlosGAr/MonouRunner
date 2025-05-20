using UnityEngine;
using System.Collections; // Importar el espacio de nombres para corrutinas

public class PlayerController : MonoBehaviour
{
    public Animator animator; // Referencia al Animator
    public float forwardSpeed = 5f; // Velocidad de avance automático
    public float laneDistance = 2f; // Distancia entre carriles
    public float jumpHeight = 2f; // Altura del salto
    public float jumpDuration = 0.5f; // Duración del salto
    public float crouchDuration = 0.5f; // Duración del agacharse
    public float stunnedDuration = 0.5f; // Duración del estado atontado

    private int currentLane = 2; // 0 = más a la izquierda, 2 = centro, 4 = más a la derecha
    private bool isJumping = false;
    private bool isCrouching = false;
    private bool isStopped = false; // Indica si el jugador está detenido
    private bool isStunned = false; // Indica si el jugador está atontado
    private bool isRight = false; // Indica si el jugador está en la dirección correcta
    private bool isLeft = false; // Indica si el jugador está en la dirección izquierda
    private Vector3 targetPosition;
    private float originalY; // Guardar la posición original en Y
    private Quaternion originalRotation; // Guardar la rotación original del jugador

    private CapsuleCollider capsuleCollider; // Referencia al CapsuleCollider
    private float originalColliderHeight; // Altura original del CapsuleCollider
    private Vector3 originalColliderCenter; // Centro original del CapsuleCollider

    private Vector2 startTouchPosition; // Posición inicial del toque
    private Vector2 endTouchPosition;   // Posición final del toque
    private bool swipeDetected = false;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>(); // Obtener el componente Animator
        targetPosition = new Vector3((currentLane - 2) * laneDistance, transform.position.y, transform.position.z); // 5 carriles
        originalY = transform.position.y; // Guardar la posición inicial en Y
        originalRotation = transform.rotation; // Guardar la rotación inicial

        // Obtener el CapsuleCollider y guardar sus valores originales
        capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            originalColliderHeight = capsuleCollider.height;
            originalColliderCenter = capsuleCollider.center;
        }
    }

    public IEnumerator HandleObstacleCollision()
    {
    // Detener el movimiento del jugador y atontarlo
    isStopped = true;
    isStunned = true;
    animator.SetBool("isStun", true);

    // Posición inicial y objetivo para el movimiento hacia atrás
    Vector3 startPosition = transform.position;
    Vector3 targetPosition = startPosition + (-Vector3.forward * 2f); // Ajusta la fuerza del empuje

    float elapsedTime = 0.2f;
    float duration = 0.3f; // Duración del movimiento hacia atrás

    // Movimiento suave hacia atrás
    while (elapsedTime < duration)
    {
        elapsedTime += Time.deltaTime;
        transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
        yield return null;
    }

    // Esperar el tiempo de atontamiento
    yield return new WaitForSeconds(stunnedDuration);

    // Restaurar la posición original en Y
    transform.position = new Vector3(transform.position.x, originalY, transform.position.z);

    // Reanudar el movimiento
    isStopped = false;

    // Finalizar el estado de atontamiento
    isStunned = false;
    animator.SetBool("isStun", false);
    }

    private void Update()
    {
        if (isStopped) return; // Detener todas las acciones si el jugador está detenido

        // Movimiento automático hacia adelante en el eje Z global
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.World);

        // Detectar gestos de deslizamiento
        DetectSwipe();

        // Calcular posición objetivo en el carril
        targetPosition = new Vector3((currentLane - 2) * laneDistance, transform.position.y, transform.position.z); // 5 carriles

        // Movimiento hacia el carril objetivo
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);

        // Salto
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping && !isCrouching)
        {
            StartCoroutine(Jump());
        }

        // Agacharse
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isCrouching && !isJumping)
        {
            StartCoroutine(Crouch());
        }
    }

    private void DetectSwipe()
    {
        if (isStunned) return; // No permitir swipes si el jugador está atontado

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
                swipeDetected = false;
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Ended)
            {
                endTouchPosition = touch.position;

                if (!swipeDetected)
                {
                    Vector2 swipeDelta = endTouchPosition - startTouchPosition;

                    if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                    {
                        // Detectar deslizamiento horizontal con 5 carriles
                        if (swipeDelta.x > 50 && currentLane < 4) // Deslizar a la derecha
                        {
                            currentLane++;
                            swipeDetected = true;
                            animator.SetBool("IsRight", true);
                            print("Swipe right detected"); // Mensaje de depuración
                            StartCoroutine(ResetIsRight());

                        }
                        else if (swipeDelta.x < -50 && currentLane > 0) // Deslizar a la izquierda
                        {
                            currentLane--;
                            swipeDetected = true;
                            animator.SetBool("IsLeft", true);
                            isLeft = true; // Indicar que el jugador está en la dirección izquierda
                            StartCoroutine(ResetIsLeft());
                        }
                    }
                    else
                    {
                        // Deslizamiento vertical
                        if (swipeDelta.y > 50 && !isJumping && !isCrouching) // Deslizar hacia arriba (saltar)
                        {
                            print("Jump detected"); // Mensaje de depuración
                            StartCoroutine(Jump());
                            swipeDetected = true;
                        }
                        else if (swipeDelta.y < -50 && !isCrouching && !isJumping) // Deslizar hacia abajo (agacharse)
                        {
                            StartCoroutine(Crouch());
                            swipeDetected = true;
                        }
                    }
                }
            }
        }
    }
    private IEnumerator ResetIsLeft()
{
    yield return new WaitForSeconds(0.3f); // Esperar 0.5 segundos
    animator.SetBool("IsLeft", false); // Restablecer el valor a falso
}
    private IEnumerator ResetIsRight()
{
    yield return new WaitForSeconds(0.3f); // Esperar 0.5 segundos
    animator.SetBool("IsRight", false); // Restablecer el valor a falso
}

    private IEnumerator Jump()
    {
        isJumping = true;
        animator.SetBool("isJumping", true); // Activar la animación de salto

        // Reducir el tamaño del CapsuleCollider durante el salto
        if (capsuleCollider != null)
        {
            capsuleCollider.height = originalColliderHeight * 0.1f; // Reducir altura
            capsuleCollider.center = originalColliderCenter + new Vector3(0, 1f, 0); // Ajustar centro
        }

        // Elevar al jugador en el eje Y
        float startY = transform.position.y;
        float targetY = startY + jumpHeight;
        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration / 2) // Subida
        {
            elapsedTime += Time.deltaTime;
            float newY = Mathf.Lerp(startY, targetY, elapsedTime / (jumpDuration / 2));
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < jumpDuration / 2) // Bajada
        {
            elapsedTime += Time.deltaTime;
            float newY = Mathf.Lerp(targetY, startY, elapsedTime / (jumpDuration / 2));
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            yield return null;
        }

        // Restaurar el tamaño original del CapsuleCollider
        if (capsuleCollider != null)
        {
            capsuleCollider.height = originalColliderHeight;
            capsuleCollider.center = originalColliderCenter;
        }

        isJumping = false;
        animator.SetBool("isJumping", false);
        print("Jump finished"); // Mensaje de depuración
    }

    private IEnumerator Crouch()
    {
        if (isCrouching) yield break; // Evita múltiples llamadas
        isCrouching = true;
        animator.SetBool("isSlide", true);
        // Reducir el tamaño del CapsuleCollider durante el agachado
        if (capsuleCollider != null)
        {
            capsuleCollider.height = originalColliderHeight * 0.2f; // Reducir altura
            capsuleCollider.center = originalColliderCenter - new Vector3(0, 1f, 0); // Ajustar centro
        }

        // Rotar hacia adelante y mover hacia abajo
        //transform.Rotate(90f, 0f, 0f);
       // transform.position += Vector3.down * 0.1f;

        yield return new WaitForSeconds(crouchDuration);

        // Restaurar el tamaño original del CapsuleCollider
        if (capsuleCollider != null)
        {
            capsuleCollider.height = originalColliderHeight;
            capsuleCollider.center = originalColliderCenter;
        }

        // Restaurar rotación y posición
       // transform.Rotate(-90f, 0f, 0f);
       // transform.position = new Vector3(transform.position.x, originalY, transform.position.z);

        isCrouching = false;
        animator.SetBool("isSlide", false);
    }
}
