using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

using UnityEngine.UI;
using System.Threading;
using Ninja.WebSockets.Exceptions;

public class LapController : NetworkBehaviour
{
    #region Variables privadas
    public PlayerInfo m_playerInfo;                 // Referencia al PlayerInfo
    private DirectionDetector m_directionDetector;  // Referencia al DirectionDetector
    private UIManager m_UIManager;                  // Referencia al UIManager
    private bool malaVuelta = false;                // Booleano que indica si la vuelta es mala (marcha atrás)
    private GameStartManager m_GSM;
    [SerializeField] private LapManager m_lapManager;
    public int num_players = 2;
    private SemaphoreSlim endSemaphore = new SemaphoreSlim(0);
    private PolePositionManager m_PPM;
    private FinishGame m_FinishGame;
    private bool gameThreadFinished;
    private bool timeEnded = false;
    private float enterArcLength;
    #endregion

    #region Variables publicas
    [HideInInspector] public bool canLap = false;                    // Bool que determina si puede sumar vueltas el jugador
    [SyncVar(hook = nameof(UpdateTimerVisually))] public float timeToEnd = 20.0f;
    #endregion


    

    void UpdateTimerVisually(float before, float after)
    {
        
    }

    #region Command Functions
    [Command]
    private void CmdUpdateTimeToEnd()
    {
        if (!m_FinishGame)
            m_FinishGame = FindObjectOfType<FinishGame>();

        m_FinishGame.RpcUpdateEndTimerText(Mathf.RoundToInt(timeToEnd));
    }

    [Command]
    private void CmdUpdatePlayerFinished(int ID)
    {
        //GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        LapManager lm = FindObjectOfType<LapManager>();

        switch(lm.nextPos)
        {
            case 0:
                lm.endPos1 = ID;
                break;
            case 1:
                lm.endPos2 = ID;
                break;
            case 2:
                lm.endPos3 = ID;
                break;
            case 3:
                lm.endPos4 = ID;
                break;
        }

        lm.nextPos++;

        switch (ID)
        {
            case 0:
                lm.player1Finished = true;
                break;
            case 1:
                lm.player2Finished = true;
                break;
            case 2:
                lm.player3Finished = true;
                break;
            case 3:
                lm.player4Finished = true;
                break;
        }
    }

    [Command]
    public void CmdUpdateReadyToShow()
    {
        if(!m_lapManager)
            m_lapManager = FindObjectOfType<LapManager>();
        m_lapManager.readyToShowFinalScreen = true;
    }

    [Command]
    public void CmdUpdateBestLap(int ID, int m, int s, int ms)
    {
        m_lapManager = FindObjectOfType<LapManager>();
        m_GSM = FindObjectOfType<GameStartManager>();
        m_PPM = FindObjectOfType<PolePositionManager>();

        string st = "";

        //Corrección string
        //Minutos
        if (m < 10)
            st += "0" + m + ":";
        else
            st += m + ":";

        //Segundos
        if (s < 10)
            st += "0" + s + ":";
        else
            st += s + ":";

        //Milisegundos
        if (ms < 10)
            st += "00" + ms;
        else if (ms < 100)
            st += "0" + ms;
        else
            st += ms;

        switch (ID)
        {
            case 0:
                m_lapManager.player1BestTimer = st;
                break;
            case 1:
                m_lapManager.player2BestTimer = st;
                break;
            case 2:
                m_lapManager.player3BestTimer = st;
                break;
            case 3:
                m_lapManager.player4BestTimer = st;
                break;
        }
    }

    [Command]
    public void CmdUpdateTimers(int ID)
    {
        //GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        //m_PPM = FindObjectOfType<PolePositionManager>();
        m_GSM = FindObjectOfType<GameStartManager>();

        string st = m_GSM.totalTimer.minutes + ":" + m_GSM.totalTimer.seconds + ":" + m_GSM.totalTimer.miliseconds;
        if(!m_lapManager)
            m_lapManager = FindObjectOfType<LapManager>();

        switch (ID)
        {
            case 0:
                m_lapManager.player1TotalTimer = st;
                break;
            case 1:
                m_lapManager.player2TotalTimer = st;
                break;
            case 2:
                m_lapManager.player3TotalTimer = st;
                break;
            case 3:
                m_lapManager.player4TotalTimer = st;
                break;
        }
        
        // Ya que daba problemas con la lista de m_PlayersNotOrdered,
        // lo he hecho con las endPos que son sincronizadas y funciona
        switch (num_players)
        {
            case 2:
                if (m_lapManager.endPos2 == ID)
                    m_lapManager.readyToShowFinalScreen = true;
                break;
            case 3:
                if (m_lapManager.endPos3 == ID)
                    m_lapManager.readyToShowFinalScreen = true;
                break;
            case 4:
                if (m_lapManager.endPos4 == ID)
                    m_lapManager.readyToShowFinalScreen = true;
                break;
        }
        
    }

    [Command]
    private void CmdUpdateEndGame()
    {
        //GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        if(!m_PPM)
            m_PPM = FindObjectOfType<PolePositionManager>();
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
    }

    private void Start()
    {
        m_playerInfo.lapBestMinutes = 0;
        m_playerInfo.lapBestSeconds = 0;
        m_playerInfo.lapBestMiliseconds = 0;
        m_lapManager = FindObjectOfType<LapManager>();
        // Se obtiene el PolePositionManager
        m_PPM = FindObjectOfType<PolePositionManager>();
        // Se obtiene el GameStartManager
        m_GSM = FindObjectOfType<GameStartManager>();
        // Se obtiene el FinishGame
        m_FinishGame = FindObjectOfType<FinishGame>();
        // Se obtiene el UIManager y se establece el texto de las vueltas
        m_UIManager = FindObjectOfType<UIManager>();
        m_UIManager.UpdateLaps(1, m_lapManager.totalLaps);
    }

    private void Update()
    {
        if (timeToEnd < 20)
            Debug.Log("TimeToEnd: " + timeToEnd);

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

        if (someoneFinished)
        {
            if (m_playerInfo.ID == 0)
            {
                timeToEnd -= Time.deltaTime;
                CmdUpdateTimeToEnd();
            }

            if (timeToEnd <= 0 && !timeEnded)
            {
                timeEnded = true;
                gameThreadFinished = true;
                CmdUpdateReadyToShow();
            }
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

        if (gameThreadFinished)
        {
            if (!m_lapManager.readyToShowFinalScreen)
                return;
            gameThreadFinished = false;
            CmdUpdateEndGame();
        }
    }

    /// <summary>
    /// Comprueba si el coche entra en un trigger
    /// </summary>
    /// <param name="other">El trigger en el que entra el coche</param>
    private void OnTriggerEnter(Collider other)
    {
        m_PPM = FindObjectOfType<PolePositionManager>();
        enterArcLength = m_PPM.m_arcLengths[m_playerInfo.CurrentPosition];
    }

    /// <summary>
    /// Comprueba si el coche sale de un trigger
    /// </summary>
    /// <param name="other">El trigger del que sale el coche</param>
    private void OnTriggerExit(Collider other)
    {

        // Si toca la meta:
        if (other.CompareTag("FinishLine") && canLap)
        {
            // Si va en buena dirección:
            if (enterArcLength > m_PPM.m_arcLengths[m_playerInfo.CurrentPosition] + 4.9f)
            {
                // Si NO ha entrado marcha atrás previamente:
                if (!malaVuelta)
                {
                    Debug.LogWarning("VUELTA CORRECTA");
                    int laps = m_playerInfo.CurrentLap + 1;
                    m_playerInfo.GetComponent<SetupPlayer>().CmdUpdateLaps(m_playerInfo.CurrentPosition, laps, m_playerInfo.ID);
                    /*string st = "LISTA PLAYERS: ";
                    for (int i = 0; i < m_PPM.m_Players.Count; i++) // Sincronizar numPlayers
                    {
                        st += m_PPM.m_Players[i].ToString() + ", ";
                    }
                    Debug.LogWarning(st);*/

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
                                CmdUpdateBestLap(m_playerInfo.ID, m_GSM.lapTimer.iMinutes, m_GSM.lapTimer.iSeconds, m_GSM.lapTimer.iMiliseconds);
                            }
                        }
                        else
                        {
                            m_playerInfo.lapBestMinutes = m_GSM.lapTimer.iMinutes;
                            m_playerInfo.lapBestSeconds = m_GSM.lapTimer.iSeconds;
                            m_playerInfo.lapBestMiliseconds = m_GSM.lapTimer.iMiliseconds;
                            CmdUpdateBestLap(m_playerInfo.ID, m_GSM.lapTimer.iMinutes, m_GSM.lapTimer.iSeconds, m_GSM.lapTimer.iMiliseconds);
                        }

                        m_GSM.lapTimer.RestartTimer();

                        // Si el jugador ha acabado la carrera
                        if (laps > m_lapManager.totalLaps)
                        {
                            // Se paran los timers y avisa de que ha terminado
                            m_GSM.lapTimer.StopTimer();
                            //m_GSM.totalTimer.StopTimer();
                            m_playerInfo.hasFinished = true;
                            CmdUpdatePlayerFinished(m_playerInfo.ID);
                            CmdUpdateTimers(m_playerInfo.ID);
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

                    m_UIManager.UpdateLaps(laps, m_lapManager.totalLaps);
                }
                // Si había entrado marcha atrás previamente:
                else
                {
                    Debug.LogWarning("RECUPERAMOS VUELTA");
                    malaVuelta = false;
                    int laps = m_playerInfo.CurrentLap + 1;
                    m_playerInfo.GetComponent<SetupPlayer>().CmdUpdateLaps(m_playerInfo.CurrentPosition, laps, m_playerInfo.ID);
                }
            }
            //Entrar bien y salir mal
            else if (enterArcLength > m_PPM.m_arcLengths[m_playerInfo.CurrentPosition])
            {
                Debug.LogWarning("Me QUEDO IGUAL 1");
            }
            //Entrar mal y salir bien
            else if (enterArcLength < m_PPM.m_arcLengths[m_playerInfo.CurrentPosition] && enterArcLength + 4.9f > m_PPM.m_arcLengths[m_playerInfo.CurrentPosition])
            {
                Debug.LogWarning("Me QUEDO IGUAL 2");
            }
            // Si entra marcha atrás:
            else
            {
                Debug.LogWarning("MALA VUELTA");
                malaVuelta = true;
                int laps = m_playerInfo.CurrentLap - 1;
                m_playerInfo.GetComponent<SetupPlayer>().CmdUpdateLaps(m_playerInfo.CurrentPosition, laps, m_playerInfo.ID);
            }
        }
    }
}