using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void Start()
    {
        // Oculta el cursor pero permite el uso normal de sus funciones
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    // Asegura que el cursor siga oculto cuando se cambie de escena
    void OnEnable()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }
}