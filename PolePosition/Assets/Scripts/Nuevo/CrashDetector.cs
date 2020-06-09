using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CrashDetector : MonoBehaviour
{
    private GameObject esfera;
    private WheelCollider[] ruedas;
    private int contadorSegundos = 0;
    private float proximoSegundo = 0;
    public LayerMask whatIsRoad;
    public float checkRadius = 0.5f;

    private void Start()
    {
        int playerID = GetComponent<PlayerInfo>().ID;
        esfera = GameObject.Find("@PolePositionManager").GetComponent<PolePositionManager>().m_DebuggingSpheres[playerID];
        ruedas = GetComponentsInChildren<WheelCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isGrounded = true;

        //Para cada rueda se comprueba si colisiona con la carretera
        for (int i = 0; i < ruedas.Length; i++)
        {
            if (!Physics.CheckSphere(ruedas[i].transform.position, checkRadius, whatIsRoad))
            {
                isGrounded = false;
                break;
            }
        }

        //Si una de las ruedas no está tocando el suelo, aumentamos el contador
        if (!isGrounded)
        {
            if (Time.time >= proximoSegundo)
            {
                contadorSegundos++;
                proximoSegundo = Time.time + 1.0f;
            }
        }
        else
        {
            contadorSegundos = 0;
        }

        //Si el contador llega a 3, se reinicia la posición del coche
        if (contadorSegundos == 3)
        {
            contadorSegundos = 0;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
            transform.position = new Vector3(esfera.transform.position.x, 3.0f, esfera.transform.position.z);
        }
    }
}
