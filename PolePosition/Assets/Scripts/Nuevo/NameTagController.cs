using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameTagController : MonoBehaviour
{
    private Transform transformCamera; // Transform de la cámara que sigue al vehículo
    public GameObject nameTag; // Name Tag del jugador (se encuentra encima del coche)

    // Start is called before the first frame update
    void Start()
    {
        transformCamera = GameObject.FindGameObjectWithTag("MainCamera").transform; // Se busca la cámara que sigue al jugador

        
    }

    // Update is called once per frame
    void Update()
    {
        if (!transformCamera)
            transformCamera = FindObjectOfType<Camera>().transform;

        nameTag.transform.LookAt(transformCamera); // El nametag del jugador mira a la cámara

        nameTag.transform.rotation = Quaternion.LookRotation(transform.position - transformCamera.position); // Se rota el nametag para que no se vea al revés
    }
}
