using Mirror.Examples.Basic;
using UnityEngine;

/// <summary>
/// Clase DirectionDetector, que comprueba si el coche va en dirección contraria.
/// </summary>
public class DirectionDetector : MonoBehaviour
{
    #region Variables privadas
    private float proximoSegundo = 0;        // Tiempo de ejecución del próximo segundo para el contador
    private float lastArcLength = 0f;        // Antigua longitud de arco
    private float firstBadArcLength = 0f;    // Primera longitud de arco que va en dirección contraria
    private bool firstSaved = false;         // Booleano que guarda si ya se tiene la primera longitud de arco mala
    private float ratioComprobacion = 0.5f;  // Ratio de comprobación de dirección
    private PolePositionManager ppm;         // Referencia al PolePositionManager
    private Rigidbody playerRB;              // Rigidbody del coche
    private bool showingInfo = false;        // Booleano para indicar si se está mostrando al jugador que va en mala dirección
    private UIManager myUIM;                 // Referencia al UIManager
    #endregion

    #region Variables publicas
    public bool buenaDireccion = true;       // Booleano para saber si va en buena dirección
    public bool haCruzadoMeta = false;       // Booleano para saber si acaba de cruzar meta
    #endregion

    /// <summary>
    /// Función Start, que inicializa las siguientes variables.
    /// </summary>
    void Start()
    {
        // Se obtiene el PolePositionManager
        ppm = FindObjectOfType<PolePositionManager>();

        // Se obtiene el rigidbody del coche
        playerRB = GetComponent<Rigidbody>();

        // Se obtiene el UIManager
        myUIM = FindObjectOfType<UIManager>();
    }

    /// <summary>
    /// Función Update, que se ejecuta cada frame.
    /// </summary>
    void Update()
    {
        //Booleano para indicar si va bien
        bool vaBien = true;

        // Cada segundo se comprueba si está avanzando hacia la meta o va en dirección contraria
        if (Time.time >= proximoSegundo)
        {
            //Si no acaba de cruzar meta, se comprueba
            if (!haCruzadoMeta)
            {
                float actualArcLength = 0f;

                // Lock para que no se solape con otros procesos
                int id = GetComponent<PlayerInfo>().ID;
                lock (ppm.xLock)
                {
                    actualArcLength = ppm.m_arcLengths[id];
                }

                // Si está yendo a hacia atrás
                if (actualArcLength < lastArcLength && playerRB.velocity.magnitude > 0.2f)
                {
                    buenaDireccion = false;

                    // Si no hemos guardado la posición de la primera dirección hacia atrás, la guardamos
                    if (!firstSaved)
                    {
                        firstSaved = true;
                        firstBadArcLength = actualArcLength;
                    }

                    // Si estamos 5 "metros" atrás de donde estábamos, estamos yendo en mala dirección
                    if (actualArcLength < firstBadArcLength - 5.0f)
                        vaBien = false;
                }
                else
                {
                    vaBien = true;
                    firstSaved = false;
                    showingInfo = false;
                    myUIM.incorrectDirection.SetActive(false);
                }

                // Actualizamos el valor de lastArcLength
                lastArcLength = actualArcLength;
            }
            //Si habia cruzado la meta, le decimos que la proxima no
            else
            {
                haCruzadoMeta = false;
            }

            // Actualizamos el tiempo de la próxima comprobación
            proximoSegundo = Time.time + ratioComprobacion;
        }

        // Si el contador llega a 3, se reinicia la posición del coche
        if (!vaBien)
        {
            if (!showingInfo)
            {
                showingInfo = true;
                myUIM.incorrectDirection.SetActive(true);
            }
        }
    }
}
