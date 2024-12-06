using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerTutorial : MonoBehaviour
{
    [Header("Configuración de movimiento")]
    public float speed = 10f;
    public float carrilOffset = 3.0f;
    private float carrilIzquierdo;
    private float carrilCentro;
    private float carrilDerecho;
    private int carrilActual = 1;
    public bool primerCambioCarril = false;

    [Header("Estado de atrapamiento")]
    public bool isCaught = false;
    private int keyPressCount = 0;
    private int requiredKeyPresses = 3; // Cantidad necesaria de pulsaciones para escapar
    public TextMeshProUGUI caughtText;
    public int escenajuego;

    public Image caughtImage; // Imagen que aparecerá cuando el jugador esté atrapado

    [Header("Control de UI")]
    private TutorialUIController tutorialUIController;

    private TutorialSpawnManager spawnManager;

    // Referencia al Animator del jugador
    private Animator playerAnimator;

    // Referencia al script MultiSenseOSCReceiver
    public MultiSenseOSCReceiver oscReceiver; // Asignar en el Inspector

    private float previousZPosition; // Para almacenar la posición Z del frame anterior

    [Header("Contador visual")]
    public Sprite[] countdownSprites; // Array de sprites (3, 2, 1)
    public Image countdownImage;      // Componente Image para mostrar los sprites

    private void Start()
    {
        carrilCentro = transform.position.x;
        carrilIzquierdo = carrilCentro - carrilOffset;
        carrilDerecho = carrilCentro + carrilOffset;

        tutorialUIController = Object.FindFirstObjectByType<TutorialUIController>();
        spawnManager = Object.FindFirstObjectByType<TutorialSpawnManager>();

        playerAnimator = GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.enabled = false; // Desactiva el Animator inicialmente
        }

        previousZPosition = transform.position.z; // Almacena la posición Z inicial

        if (caughtText != null) caughtText.gameObject.SetActive(false);
        if (caughtImage != null) caughtImage.gameObject.SetActive(false); // Asegurarse de que la imagen está desactivada al inicio
        if (countdownImage != null) countdownImage.gameObject.SetActive(false); // Ocultar contador al inicio
    }

    private void Update()
    {
        if (isCaught)
        {
            HandleCaughtState();
            
            // Mostrar imagen de atrapado
            if (caughtImage != null && !caughtImage.gameObject.activeSelf)
            {
                caughtImage.gameObject.SetActive(true);
            }

            // Obtener valores del acelerómetro desde el script MultiSenseOSCReceiver
            float accelX = oscReceiver.accelX;
            float accelY = oscReceiver.accelY;
            float accelZ = oscReceiver.accelZ;

            // Umbral para detectar movimiento significativo
            float threshold = 15f;

            // Liberar al jugador si el movimiento del celular supera el umbral
            if (Mathf.Abs(accelX) > threshold || Mathf.Abs(accelY) > threshold || Mathf.Abs(accelZ) > threshold)
            {
                ReleasePlayer();
            }
        }
        else
        {
            // Ocultar imagen de atrapado si no está atrapado
            if (caughtImage != null && caughtImage.gameObject.activeSelf)
            {
                caughtImage.gameObject.SetActive(false);
            }
            CheckMovement();
            MoveForward();
            HandleLaneChange();
        }
    }

    private void CheckMovement()
    {
        // Compara la posición Z actual con la posición Z del frame anterior
        bool isMoving = Mathf.Abs(transform.position.z - previousZPosition) > 0.01f;

        if (playerAnimator != null)
        {
            playerAnimator.enabled = isMoving; // Activa o desactiva el Animator dependiendo del movimiento
        }

        previousZPosition = transform.position.z; // Actualiza la posición Z para el siguiente frame
    }

    private void MoveForward()
    {
        if (!TutorialWall.isPaused && primerCambioCarril) // Solo avanzar si el juego no está en pausa y primerCambioCarril es true
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    private void HandleLaneChange()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            CambiarCarril(0);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            CambiarCarril(1);
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            CambiarCarril(2);
        }
    }

    private void CambiarCarril(int nuevoCarril)
    {
        if (nuevoCarril == carrilActual) return;

        carrilActual = nuevoCarril;
        Vector3 nuevaPosicion = transform.position;

        if (carrilActual == 0)
            nuevaPosicion.x = carrilIzquierdo;
        else if (carrilActual == 1)
            nuevaPosicion.x = carrilCentro;
        else if (carrilActual == 2)
            nuevaPosicion.x = carrilDerecho;

        transform.position = nuevaPosicion;

        if (!primerCambioCarril)
        {
            primerCambioCarril = true;

            if (tutorialUIController != null)
            {
                tutorialUIController.OcultarInstrucciones();
            }

            if (spawnManager != null)
            {
                spawnManager.SpawnWalls();
            }
        }
    }

    public void StartCaughtState()
    {
        isCaught = true;
        keyPressCount = 0;

        if (caughtText != null)
        {
            caughtText.gameObject.SetActive(true);
            caughtText.text = $"¡Estás atrapado! Presiona E {requiredKeyPresses - keyPressCount} veces para liberarte.";
        }

        if (tutorialUIController != null)
        {
            tutorialUIController.MostrarAtrapadoImagen();
            tutorialUIController.OcultarSegundaImagen();
        }

        Debug.Log("¡Estás atrapado! Presiona E para liberarte.");
    }

    private void HandleCaughtState()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            keyPressCount++;
            Debug.Log($"Presionaste E, conteo: {keyPressCount}");

            if (caughtText != null)
            {
                caughtText.text = $"¡Estás atrapado! Presiona E {requiredKeyPresses - keyPressCount} veces más";
            }
        }

        if (keyPressCount >= requiredKeyPresses)
        {
            ReleasePlayer();
        }
    }

    private void ReleasePlayer()
    {
        isCaught = false;
        keyPressCount = 0;

        if (caughtText != null)
        {
            caughtText.gameObject.SetActive(false);
        }

        if (tutorialUIController != null)
        {
            tutorialUIController.OcultarAtrapadoImagen();
        }

        Debug.Log("¡Te has liberado!");

        StartCoroutine(PostReleaseAction());
    }

    private System.Collections.IEnumerator PostReleaseAction()
    {
        // Esperar 2 segundos antes de iniciar el conteo
        yield return new WaitForSeconds(2f);

        if (countdownImage != null) countdownImage.gameObject.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            if (countdownSprites.Length >= i)
            {
                countdownImage.sprite = countdownSprites[i - 1];
            }
        yield return new WaitForSeconds(1f);
        }

        if (countdownImage != null) countdownImage.gameObject.SetActive(false);

        // Carga de la nueva escena
        SceneManager.LoadScene(1);
    }
}