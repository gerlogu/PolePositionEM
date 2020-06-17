using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

using UnityEngine.UI;
using System.Threading;

public class LapController : NetworkBehaviour
{
    #region Variables privadas
    public PlayerInfo m_playerInfo;                 // Referencia al PlayerInfo
    private DirectionDetector m_directionDetector;  // Referencia al DirectionDetector
    private UIManager m_UIManager;                  // Referencia al UIManager
    private bool malaVuelta = false;                // Booleano que indica si la vuelta es mala (marcha atrás)
    private GameStartManager m_GSM;
    private LapManager m_lapManager;
    public int num_players = 2;
    private SemaphoreSlim endSemaphore = new SemaphoreSlim(0);
    private PolePositionManager m_PPM;
    private FinishGame m_FinishGame;
    private bool gameThreadFinished;
    #endregion

    #region Variables publicas
    [Tooltip("Número total de vueltas")] public int totalLaps = 3; // Por poner algo de momento
    [HideInInspector] public bool canLap = false;                     // Bool que determina si puede sumar vueltas el jugador
    [SyncVar] public float timeToEnd = 20.0f;
    #endregion

    #region Command Functions
    [Command]
    private void CmdUpdateTimeToEnd()
    {
        //GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        timeToEnd -= Time.deltaTime;
        //Debug.LogWarning("TIEMPO: " + timeToEnd);
        m_FinishGame.RpcUpdateEndTimerText(Mathf.RoundToInt(timeToEnd));
    }

    [Command]
    private void CmdUpdatePlayerFinished(int ID)
    {
        //GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        switch (ID)
        {
            case 0:
                m_lapManager.player1Finished = true;
                break;
            case 1:
                m_lapManager.player2Finished = true;
                break;
            case 2:
                m_lapManager.player3Finished = true;
                break;
            case 3:
                m_lapManager.player4Finished = true;
                break;
        }
    }

    [Command]
    private void CmdUpdateEndGame()
    {
        //GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        m_PPM.gameHasEnded = true;
    }
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

        m_lapManager = FindObjectOfType<LapManager>();
        // Se obtiene el PolePositionManager
        m_PPM = FindObjectOfType<PolePositionManager>();

        // Se obtiene el FinishGame
        m_FinishGame = FindObjectOfType<FinishGame>();
    }

    private void Start()
    {
        m_playerInfo.lapBestMinutes = 0;
        m_playerInfo.lapBestSeconds = 0;
        m_playerInfo.lapBestMiliseconds = 0;
    }

    private void Update()
    {
        if (m_playerInfo.canMove)
        {
            m_playerInfo.lapTotalMinutes = m_GSM.totalTimer.minutes;
            m_playerInfo.lapTotalSeconds = m_GSM.totalTimer.seconds;
            m_playerInfo.lapTotalMiliseconds = m_GSM.totalTimer.miliseconds;
        }

        bool[] playersFinished = { m_lapManager.player1Finished, m_lapManager.player2Finished, m_lapManager.player2Finished, m_lapManager.player4Finished };
        bool someoneFinished = false;

        for (int i = 0; i < m_PPM.m_PlayersNotOrdered.Count; i++)
        {
            if (playersFinished[i])
            {
                someoneFinished = true;
            }
        }

        //Debug.LogWarning("SOMEONE FINISHED: " + someoneFinished);

        if (someoneFinished)
        {
            if (m_playerInfo.ID == 0)
                CmdUpdateTimeToEnd();
        }

        if (gameThreadFinished)
        {
            gameThreadFinished = false;
            CmdUpdateEndGame();
        }

        int[] playerLaps = { m_lapManager.player1Laps, m_lapManager.player2Laps, m_lapManager.player3Laps, m_lapManager.player4Laps };

        for (int i = 0; i < m_PPM.m_PlayersNotOrdered.Count; i++)
        {
            //m_PPM.m_Players[m_PPM.m_Players[i].CurrentPosition].CurrentLap = playerLaps[i];
            m_PPM.m_PlayersNotOrdered[i].CurrentLap = playerLaps[i];
        }

        bool[] playerFinished = { m_lapManager.player1Finished, m_lapManager.player2Finished, m_lapManager.player2Finished, m_lapManager.player4Finished };

        for (int i = 0; i < m_PPM.m_PlayersNotOrdered.Count; i++)
        {
            m_PPM.m_PlayersNotOrdered[i].hasFinished = playerFinished[i];
        }
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
                    int laps = m_playerInfo.CurrentLap + 1;
                    m_playerInfo.GetComponent<SetupPlayer>().CmdUpdateLaps(m_playerInfo.CurrentPosition, laps, m_playerInfo.ID);
                    string st = "LISTA PLAYERS: ";
                    for (int i = 0; i < num_players; i++)
                    {
                        st += m_PPM.m_Players[i].ToString() + ", ";
                    }
                    Debug.LogWarning(st);

                    if (laps > 1)
                    {
                        if (laps > 2)
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

                        // Si el jugador ha acabado la carrera
                        if (laps > totalLaps)
                        {
                            // Se paran los timers y avisa de que ha terminado
                            m_GSM.lapTimer.StopTimer();
                            m_GSM.totalTimer.StopTimer();
                            m_playerInfo.hasFinished = true;
                            CmdUpdatePlayerFinished(m_playerInfo.ID);
                            m_playerInfo.canMove = false;
                            m_UIManager.waitFinishHUD.SetActive(true);
                            m_UIManager.inGameHUD.SetActive(false);

                            // Hilo de espera a finalizar la carrera
                            Thread endGameThread = new Thread(() =>
                            {
                                // Si es el último, libera los permisos
                                if (m_playerInfo.CurrentPosition == num_players - 1)
                                {
                                    endSemaphore.Release(num_players - 1);
                                }
                                // Si no, se espera el tiempo que falte
                                else
                                {
                                    endSemaphore.Wait(Mathf.RoundToInt(timeToEnd * 1000));
                                }
                                // Al acabar de esperar, acaba la partida
                                gameThreadFinished = true;
                            });

                            endGameThread.Start();
                        }
                    }

                    m_UIManager.UpdateLaps(laps, totalLaps);
                    m_directionDetector.haCruzadoMeta = true;
                }
                // Si había entrado marcha atrás previamente:
                else
                {
                    malaVuelta = false;
                    int laps = m_playerInfo.CurrentLap + 1;
                    m_playerInfo.GetComponent<SetupPlayer>().CmdUpdateLaps(m_playerInfo.CurrentPosition, laps, m_playerInfo.ID);
                }
            }
            // Si entra marcha atrás:
            else
            {
                malaVuelta = true;
                int laps = m_playerInfo.CurrentLap - 1;
                m_playerInfo.GetComponent<SetupPlayer>().CmdUpdateLaps(m_playerInfo.CurrentPosition, laps, m_playerInfo.ID);
            }
        }
    }
}