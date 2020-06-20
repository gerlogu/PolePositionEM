using UnityEngine;
using Mirror;

using System.Threading;

/// <summary>
/// Clase que controla las vueltas
/// </summary>
public class LapController : NetworkBehaviour
{
    #region Variables privadas
    private bool malaVuelta = false;                            // Booleano que indica si la vuelta es mala (marcha atrás)
    private bool gameThreadFinished;                            // Booleano que indica si el thread de fin de partida ha acabado
    private bool timeEnded = false;                             // Booleano que indica si se ha acabado el tiempo
    private float enterArcLength;                               // Variable decimal que guarda la longitud de arco al entrar en el trigger de la meta
    private PlayerInfo m_playerInfo;                            // Referencia al PlayerInfo
    private DirectionDetector m_directionDetector;              // Referencia al DirectionDetector
    private UIManager m_UIManager;                              // Referencia al UIManager
    private GameStartManager m_GSM;                             // Referencia al GameStartManager
    private LapManager m_lapManager;                            // Referencia al LapManager
    private PolePositionManager m_PPM;                          // Referencia al PolePositionManager
    private FinishGame m_FinishGame;                            // Referencia al FinishGame
    private SemaphoreSlim endSemaphore = new SemaphoreSlim(0);  // Semáforo para gestionar el final de partida

    #endregion

    #region Command Functions
    /// <summary>
    /// Función que llama a actualizar el texto de tiempo restante
    /// </summary>
    [Command]
    private void CmdUpdateTimeToEnd()
    {
        if (!m_FinishGame)
            m_FinishGame = FindObjectOfType<FinishGame>();

        m_FinishGame.RpcUpdateEndTimerText(Mathf.RoundToInt(m_lapManager.timeToEnd));
    }

    /// <summary>
    /// Función que avisa de que el jugador ID ha terminado la carrera
    /// </summary>
    /// <param name="ID">Identificador del coche que ha terminado</param>
    [Command]
    private void CmdUpdatePlayerFinished(int ID)
    {
        // Se obtiene una referencia al LapManager
        LapManager lm = FindObjectOfType<LapManager>();

        // Se guarda al jugador en la siguiente posición final
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

        // Se modifica el booleano que indica que ha acabado
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

    /// <summary>
    /// Función que avisa de que ya está listo para mostrar la pantalla final
    /// </summary>
    [Command]
    public void CmdUpdateReadyToShow()
    {
        if(!m_lapManager)
            m_lapManager = FindObjectOfType<LapManager>();
        m_lapManager.readyToShowFinalScreen = true;
    }

    /// <summary>
    /// Función que guarda la mejor vuelta del jugador ID
    /// </summary>
    /// <param name="ID">Identificador del coche</param>
    /// <param name="m">Minutos</param>
    /// <param name="s">Segundos</param>
    /// <param name="ms">Milisegundos</param>
    [Command]
    public void CmdUpdateBestLap(int ID, int m, int s, int ms)
    {
        // Se toman las referencias necesarias
        m_lapManager = FindObjectOfType<LapManager>();
        m_GSM = FindObjectOfType<GameStartManager>();
        m_PPM = FindObjectOfType<PolePositionManager>();

        // Se crea una string con los valores
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

        // Se guarda la string en el mejor tiempo del jugador ID
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

    /// <summary>
    /// Función que guarda el tiempo total del jugador ID
    /// </summary>
    /// <param name="ID">Identificador del coche que actualiza su tiempo</param>
    [Command]
    public void CmdUpdateTimers(int ID)
    {
        // Se toman las referencias necesarias
        m_GSM = FindObjectOfType<GameStartManager>();
        m_lapManager = FindObjectOfType<LapManager>();

        // Se guarda en una string el tiempo total
        string st = m_GSM.totalTimer.minutes + ":" + m_GSM.totalTimer.seconds + ":" + m_GSM.totalTimer.miliseconds;

        // Se asigna el tiempo total al jugador ID
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
        
        // Si el jugador es el último, la partida acaba
        switch (m_GSM.numPlayers)
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

        // Si solo queda un jugador (el host), la partida acaba
        if (FindObjectOfType<NetworkManager>().numPlayers == 1)
            m_lapManager.readyToShowFinalScreen = true;
    }

    /// <summary>
    /// Función que indica que la partida ha terminado
    /// </summary>
    [Command]
    private void CmdUpdateEndGame()
    {
        // Se toma la referencia al PolePositionManager
        m_PPM = FindObjectOfType<PolePositionManager>();

        // Se actualiza la variable compartida gameHasEnded del PolePositionManager
        m_PPM.gameHasEnded = true;
    }
    #endregion

    #region Unity Callbacks
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
        // Se inicializa el tiempo de la mejor vuelta a unos valores maximos
        m_playerInfo.lapBestMinutes = 99;
        m_playerInfo.lapBestSeconds = 99;
        m_playerInfo.lapBestMiliseconds = 999;

        // Se obtiene el Lap Manager
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

    /// <summary>
    /// Función Update, que se ejecuta cada frame
    /// </summary>
    private void Update()
    {
        // Si solo queda un jugador una vez la partida ha empezado, acaba la partida
        if (FindObjectOfType<NetworkManager>().numPlayers == 1 && m_GSM.gameStarted)
        {
            m_playerInfo.hasFinished = true;
            CmdUpdatePlayerFinished(m_playerInfo.ID);
            CmdUpdateTimers(m_playerInfo.ID);
            gameThreadFinished = true;
        }

        // Si el jugador se puede mover, guardamos su tiempo en el player info
        if (m_playerInfo.canMove)
        {
            m_playerInfo.lapTotalMinutes = m_GSM.totalTimer.minutes;
            m_playerInfo.lapTotalSeconds = m_GSM.totalTimer.seconds;
            m_playerInfo.lapTotalMiliseconds = m_GSM.totalTimer.miliseconds;
        }

        // Comprobamos si alguien ha acabado la carrera
        bool[] playersFinished = { m_lapManager.player1Finished, m_lapManager.player2Finished, m_lapManager.player2Finished, m_lapManager.player4Finished };
        bool someoneFinished = false;

        for (int i = 0; i < m_PPM.m_PlayersNotOrdered.Count; i++)
        {
            if (playersFinished[i])
            {
                someoneFinished = true;
            }
        }

        // Si alguien ha acabado
        if (someoneFinished)
        {
            // Si es el servidor, actualiza el tiempo que falta para acabar
            if (isServer)
            {
                m_lapManager.timeToEnd -= Time.deltaTime;
                CmdUpdateTimeToEnd();
            }

            // Si no queda tiempo para acabar, mostramos la información por pantalla
            if (m_lapManager.timeToEnd <= 0 && !timeEnded)
            {
                timeEnded = true;
                gameThreadFinished = true;
                CmdUpdateReadyToShow();
            }
        }

        // Se obtienen las vueltas de cada jugador
        int[] playerLaps = { m_lapManager.player1Laps, m_lapManager.player2Laps, m_lapManager.player3Laps, m_lapManager.player4Laps };

        for (int i = 0; i < m_PPM.m_PlayersNotOrdered.Count; i++)
        {
            m_PPM.m_PlayersNotOrdered[i].CurrentLap = playerLaps[i];
        }

        // Si el thread de fin de partida ha acabado
        if (gameThreadFinished)
        {
            // Si no está listo para mostrar la pantalla final, no continuamos
            if (!m_lapManager.readyToShowFinalScreen)
                return;

            // Si ya está listo, acaba la partida
            gameThreadFinished = false;
            CmdUpdateEndGame();
        }

        // Se obtiene la información de si ha acabado cada jugador
        bool[] playerFinished = { m_lapManager.player1Finished, m_lapManager.player2Finished, m_lapManager.player3Finished, m_lapManager.player4Finished };

        for (int i = 0; i < m_PPM.m_PlayersNotOrdered.Count; i++)
        {
            m_PPM.m_PlayersNotOrdered[i].hasFinished = playerFinished[i];
        }
    }

    #endregion

    #region Trigger Functions

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
        // Si la partida no ha acabado
        if (!m_PPM.gameHasEnded)
        {
            // Si toca la meta:
            if (other.CompareTag("FinishLine") && isLocalPlayer)
            {
                // Si va en buena dirección:
                if (enterArcLength > m_PPM.m_arcLengths[m_playerInfo.CurrentPosition] + 4.9f)
                {
                    // Si NO ha entrado marcha atrás previamente:
                    if (!malaVuelta)
                    {
                        // Actualizamos las vueltas del jugador
                        int laps = m_playerInfo.CurrentLap + 1;
                        m_playerInfo.GetComponent<SetupPlayer>().CmdUpdateLaps(laps, m_playerInfo.ID);

                        // Si lleva más de una vuelta
                        if (laps > 1)
                        {
                            // Si lleva más de dos vueltas
                            if (laps > 2)
                            {
                                // Comprobamos si esta vuelta ha sido la mejor
                                bool isBetter = false;

                                m_GSM = FindObjectOfType<GameStartManager>();
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

                                // Si es mejor que la guardada, la actualizamos
                                if (isBetter)
                                {
                                    m_playerInfo.lapBestMinutes = m_GSM.lapTimer.iMinutes;
                                    m_playerInfo.lapBestSeconds = m_GSM.lapTimer.iSeconds;
                                    m_playerInfo.lapBestMiliseconds = m_GSM.lapTimer.iMiliseconds;
                                    CmdUpdateBestLap(m_playerInfo.ID, m_GSM.lapTimer.iMinutes, m_GSM.lapTimer.iSeconds, m_GSM.lapTimer.iMiliseconds);
                                }
                            }
                            // Si acaba de hacer la primera vuelta, guardamos ese tiempo como el mejor
                            else
                            {
                                m_playerInfo = GetComponent<PlayerInfo>();
                                m_GSM = FindObjectOfType<GameStartManager>();
                                m_playerInfo.lapBestMinutes = m_GSM.lapTimer.iMinutes;
                                m_playerInfo.lapBestSeconds = m_GSM.lapTimer.iSeconds;
                                m_playerInfo.lapBestMiliseconds = m_GSM.lapTimer.iMiliseconds;
                                CmdUpdateBestLap(m_playerInfo.ID, m_GSM.lapTimer.iMinutes, m_GSM.lapTimer.iSeconds, m_GSM.lapTimer.iMiliseconds);
                            }

                            m_GSM = FindObjectOfType<GameStartManager>();
                            // Reiniciamos el timer de vuelta
                            m_GSM.lapTimer.RestartTimer();

                            m_lapManager = FindObjectOfType<LapManager>();
                            // Si el jugador ha acabado la carrera
                            if (laps > m_lapManager.totalLaps)
                            {
                                // Se para el timer de vuelta y avisa de que ha terminado
                                m_GSM.lapTimer.StopTimer();
                                m_playerInfo.hasFinished = true;
                                CmdUpdatePlayerFinished(m_playerInfo.ID);
                                CmdUpdateTimers(m_playerInfo.ID);

                                // Se le impide moverse al jugador
                                m_playerInfo.canMove = false;

                                m_UIManager = FindObjectOfType<UIManager>();
                                // Se muestra la UI correspondiente
                                m_UIManager.waitFinishHUD.SetActive(true);
                                m_UIManager.inGameHUD.SetActive(false);

                                // Hilo de espera a finalizar la carrera
                                Thread endGameThread = new Thread(() =>
                                {
                                // Si es el último, libera los permisos
                                if (m_playerInfo.CurrentPosition == m_GSM.numPlayers - 1)
                                    {
                                        endSemaphore.Release(m_GSM.numPlayers - 1);
                                    }
                                // Si no, se espera el tiempo que falte
                                else
                                    {
                                        endSemaphore.Wait(Mathf.RoundToInt(m_lapManager.timeToEnd * 1000));
                                    }
                                // Al acabar de esperar, acaba la partida
                                gameThreadFinished = true;
                                });

                                // Comienza la ejecución del hilo
                                endGameThread.Start();
                            }
                        }

                        // Se actualizan las vueltas en la interfaz
                        m_UIManager.UpdateLaps(laps, m_lapManager.totalLaps);
                    }

                    // Si había entrado marcha atrás previamente
                    else
                    {
                        malaVuelta = false;
                        int laps = m_playerInfo.CurrentLap + 1;
                        m_playerInfo.GetComponent<SetupPlayer>().CmdUpdateLaps(laps, m_playerInfo.ID);
                    }
                }
                //Entrar bien y salir mal
                else if (enterArcLength > m_PPM.m_arcLengths[m_playerInfo.CurrentPosition])
                {
                    // Tenemos que dejar esta comprobación
                }
                //Entrar mal y salir bien
                else if (enterArcLength < m_PPM.m_arcLengths[m_playerInfo.CurrentPosition] && enterArcLength + 4.9f > m_PPM.m_arcLengths[m_playerInfo.CurrentPosition])
                {
                    // Tenemos que dejar esta comprobación
                }
                // Si entra marcha atrás
                else
                {
                    malaVuelta = true;
                    int laps = m_playerInfo.CurrentLap - 1;
                    m_playerInfo.GetComponent<SetupPlayer>().CmdUpdateLaps(laps, m_playerInfo.ID);
                }
            }
        }
    }

    #endregion
}