using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

using UnityEngine.UI;

public class LapController : NetworkBehaviour
{
    #region Variables privadas
    public PlayerInfo m_playerInfo;                 // Referencia al PlayerInfo
    private DirectionDetector m_directionDetector;  // Referencia al DirectionDetector
    private UIManager m_UIManager;                  // Referencia al UIManager
    private bool malaVuelta = false;                // Booleano que indica si la vuelta es mala (marcha atrás)
    private GameStartManager m_GSM;
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

        // Se obtiene el GameStartManager
        m_GSM = FindObjectOfType<GameStartManager>();
    }

    private void Start()
    {
        m_playerInfo.lapBestMinutes = 0;
        m_playerInfo.lapBestSeconds = 0;
        m_playerInfo.lapBestMiliseconds = 0;
    }

    private void Update()
    {
        if (!m_playerInfo.canMove)
            return;

        m_playerInfo.lapTotalMinutes = m_GSM.totalTimer.minutes;
        m_playerInfo.lapTotalSeconds = m_GSM.totalTimer.seconds;
        m_playerInfo.lapTotalMiliseconds = m_GSM.totalTimer.miliseconds;
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
                    if (m_playerInfo.CurrentLap > 1)
                    {
                        if (m_playerInfo.CurrentLap > 2)
                        {
                            bool isBetter = false;

                            if (m_GSM.lapTimer.iMinutes < m_playerInfo.lapBestMinutes)
                            {
                                isBetter = true;
                            }
                            else if (m_GSM.lapTimer.iSeconds < m_playerInfo.lapBestSeconds && m_GSM.lapTimer.iMinutes == m_playerInfo.lapBestMinutes)
                            {
                                isBetter = true;
                            }
                            else if (m_GSM.lapTimer.iMiliseconds < m_playerInfo.lapBestMiliseconds && m_GSM.lapTimer.iMinutes == m_playerInfo.lapBestMinutes && m_GSM.lapTimer.iSeconds == m_playerInfo.lapBestSeconds)
                            {
                                isBetter = true;
                            }

                            if (isBetter)
                            {
                                /*Debug.LogWarning("Vuelta previa: " + m_playerInfo.lapBestMinutes + ":" + m_playerInfo.lapBestSeconds + ":" + m_playerInfo.lapBestMiliseconds
                                    + " | Vuelta mejor: " + m_GSM.lapTimer.iMinutes + ":" + m_GSM.lapTimer.iSeconds + ":" + m_GSM.lapTimer.iMiliseconds);*/
                                m_playerInfo.lapBestMinutes = m_GSM.lapTimer.iMinutes;
                                m_playerInfo.lapBestSeconds = m_GSM.lapTimer.iSeconds;
                                m_playerInfo.lapBestMiliseconds = m_GSM.lapTimer.iMiliseconds;
                            }
                        }
                        else
                        {
                            m_playerInfo.lapBestMinutes = m_GSM.lapTimer.iMinutes;
                            m_playerInfo.lapBestSeconds = m_GSM.lapTimer.iSeconds;
                            m_playerInfo.lapBestMiliseconds = m_GSM.lapTimer.iMiliseconds;
                        }

                        m_GSM.lapTimer.RestartTimer();
                        if (m_playerInfo.CurrentLap > totalLaps)
                        {
                            m_GSM.lapTimer.StopTimer();
                            m_GSM.totalTimer.StopTimer();
                            m_playerInfo.hasFinished = true;
                            FindObjectOfType<PolePositionManager>().gameHasEnded = true;
                        }
                    }
                        
                    m_UIManager.UpdateLaps(m_playerInfo.CurrentLap, totalLaps);
                    m_directionDetector.haCruzadoMeta = true;

                    
                            
                }
                // Si había entrado marcha atrás previamente:
                else
                {
                    malaVuelta = false;
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