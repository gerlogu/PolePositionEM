using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Diagnostics;
using UnityEngine.UI;

public class LapController : NetworkBehaviour
{
    #region Variables privadas
    public PlayerInfo m_playerInfo;                 // Referencia al PlayerInfo
    private DirectionDetector m_directionDetector;  // Referencia al DirectionDetector
    private UIManager m_UIManager;                  // Referencia al UIManager
    private bool malaVuelta = false;                // Booleano que indica si la vuelta es mala (marcha atrás)
    private Stopwatch timer = new Stopwatch();                        // Timer de la vuelta
    private Text timerText;
    #endregion

    #region Variables publicas
    [Tooltip("Número total de vueltas")] public int totalLaps = 3; // Por poner algo de momento
    [HideInInspector] public bool canLap = false;                     // Bool que determina si puede sumar vueltas el jugador
    #endregion

    /// <summary>
    /// Función Awake, que inicializa las siguientes variables.
    /// </summary>
    void Awake()
    {
        // Se obtiene el PlayerInfo
        m_playerInfo = this.GetComponent<PlayerInfo>();

        // Se obtiene el DirectionDetector
        m_directionDetector = this.GetComponent<DirectionDetector>();

        // Se obtiene el UIManager y se establece el texto de las vueltas
        m_UIManager = FindObjectOfType<UIManager>();
        m_UIManager.UpdateLaps(1, totalLaps);
    }

    private void Start()
    {
        timerText = GameObject.FindGameObjectWithTag("LapTimer").GetComponent<Text>();
        timer.Start();
    }

    void StartTimer()
    {
        
    }

    private void Update()
    {
        if (!timerText)
            return;

        int hours       = Mathf.RoundToInt((float)timer.Elapsed.TotalHours);
        int minutes     = Mathf.RoundToInt((float)timer.Elapsed.Minutes);
        int seconds     = Mathf.RoundToInt((float)timer.Elapsed.Seconds);
        int miliseconds = Mathf.RoundToInt((float)timer.Elapsed.Milliseconds);

        string sHours = "0";

        if (hours < 10)
        {
            sHours = "0" + hours;
        }
        else
        {
            sHours = hours.ToString();
        }

        string sMinutes = "0";

        if (minutes < 10)
        {
            sMinutes = "0" + minutes;
        }
        else
        {
            sMinutes = minutes.ToString();
        }

        string sSeconds = "0";

        if (seconds < 10)
        {
            sSeconds = "0" + seconds;
        }
        else
        {
            sSeconds = seconds.ToString();
        }

        string sMiliseconds = "0";

        if (miliseconds < 10)
        {
            sMiliseconds = "00" + miliseconds;
        }
        else if(miliseconds < 100)
        {
            sMiliseconds = "0" + miliseconds;
        }
        else
        {
            sMiliseconds = miliseconds.ToString();
        }

        timerText.text = "Lap time: " + sMinutes + ":" + sSeconds + ":" + sMiliseconds;
    }

    /// <summary>
    /// Comprueba si el coche entra en un trigger
    /// </summary>
    /// <param name="other">El trigger en el que entra el coche</param>
    private void OnTriggerEnter(Collider other)
    {
        // (Vale a ver esto es un problema a ver si tu sabes arreglarlo)
        // El host funciona perfecto, pero el cliente no porque tiene el lapController desactivado
        // La cosa es que el cliente me está modificando las vueltas del host y no debe

        // Si toca la meta:
        if (other.CompareTag("FinishLine") && canLap)
        {
            // Si va en buena dirección:
            if (m_directionDetector.buenaDireccion)
            {
                // Si NO ha entrado marcha atrás previamente:
                if (!malaVuelta)
                {
                    m_playerInfo.CurrentLap++;
                    timer.Stop();
                    m_UIManager.UpdateLaps(m_playerInfo.CurrentLap, totalLaps);
                    m_directionDetector.haCruzadoMeta = true;
                }
                // Si había entrado marcha atrás previamente:
                else
                {
                    malaVuelta = false;
                    timer.Stop();
                    m_playerInfo.CurrentLap++;
                }
            }
            // Si entra marcha atrás:
            else
            {
                malaVuelta = true;
                m_playerInfo.CurrentLap--;
            }
        }
    }
}