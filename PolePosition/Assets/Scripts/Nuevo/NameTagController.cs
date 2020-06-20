using UnityEngine;

/// <summary>
/// Controlador de posición para los name tags de los jugadores.
/// </summary>
public class NameTagController : MonoBehaviour
{
    private Transform transformCamera; // Transform de la cámara que sigue al vehículo
    public GameObject nameTag;         // Name Tag del jugador (se encuentra encima del coche)

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        transformCamera = GameObject.FindGameObjectWithTag("MainCamera").transform; // Se busca la cámara que sigue al jugador
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (!transformCamera)
            transformCamera = FindObjectOfType<Camera>().transform;

        nameTag.transform.LookAt(transformCamera); // El nametag del jugador mira a la cámara

        nameTag.transform.rotation = Quaternion.LookRotation(transform.position - transformCamera.position); // Se rota el nametag para que no se vea al revés
    }
}
