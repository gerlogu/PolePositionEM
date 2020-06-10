using UnityEngine;

//Clase CrashDetector, que comprueba si el coche ha tenido una colisión.
public class CrashDetector : MonoBehaviour
{
    #region Variables privadas
    private GameObject esfera; //Esfera que se mueve junto al coche
    private WheelCollider[] ruedas; //Array de ruedas del coche
    private int contadorSegundos = 0; //Contador de segundos para cuando el coche ha tenido una colisión
    private float proximoSegundo = 0; //Tiempo de ejecución del próximo segundo para el contador
    private Transform transformCamera; //Transform de la cámara
    private Rigidbody playerRB; //Rigidbody del coche
    #endregion

    #region Variables publicas
    [Tooltip("Layer \"Road\"")] public LayerMask whatIsRoad; //Capa que indica qué es carretera
    [Tooltip("Radio de detección de la carretera")] public float checkRadius = 0.5f; //Radio de detección de la carretera
    #endregion

    //Función Start, que inicializa las siguientes variables.
    private void Start()
    {
        //Se obtiene la esfera del jugador del array que se encuentra en el PolePositionManager
        int playerID = GetComponent<PlayerInfo>().ID;
        esfera = GameObject.Find("@PolePositionManager").GetComponent<PolePositionManager>().m_DebuggingSpheres[playerID];

        //Se obtiene la cámara que sigue al jugador
        transformCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;

        //Se guardan las ruedas en el array
        ruedas = GetComponentsInChildren<WheelCollider>();

        //Se obtiene el rigidbody del coche
        playerRB = GetComponent<Rigidbody>();
    }

    //Función Update, que se ejecuta cada frame.
    void Update()
    {
        //Variable booleana para saber si el coche está tocando el suelo
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

        //Si una de las ruedas no está tocando el suelo, se aumenta el contador
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
            //Si el coche está en el suelo, pero no puede moverse, también se considera
            float InputAcceleration = Input.GetAxis("vertical");
            if (playerRB.velocity.magnitude > 0.2f && InputAcceleration != 0)
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
        }

        //Si el contador llega a 3, se reinicia la posición del coche
        if (contadorSegundos == 3)
        {
            contadorSegundos = 0;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transformCamera.rotation.eulerAngles.y, 0);
            transform.position = new Vector3(esfera.transform.position.x, 3.0f, esfera.transform.position.z);
        }
    }
}