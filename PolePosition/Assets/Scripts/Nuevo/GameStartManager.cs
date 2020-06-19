using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;
using Mirror;
using System;
using UnityEngine.UI;

public class GameStartManager : NetworkBehaviour
{
    #region Variables Públicas
    [Tooltip("Timer de la vuelta")]
    public LapTimer lapTimer = new LapTimer();
    [Tooltip("Timer total de la partida")]
    public LapTimer totalTimer = new LapTimer();
    [Tooltip("Referencia del Pole Position Manager")]
    public PolePositionManager m_PolePositionManager;

    #region Variables Sincronizadas Ocultas
    [Tooltip("Jugadores requeridos para que empiece la partida")]
    [HideInInspector] [SyncVar] public int minPlayers;
    [HideInInspector] [SyncVar(hook = nameof(H_SetGameStarted))] public bool gameStarted = false; // Estado de la partida
    [HideInInspector] [SyncVar(hook = nameof(H_UpdatePlayers))] public int numPlayers = 0;        // Números de jugadores listos tras finalizar el timer
    [HideInInspector] [SyncVar(hook = nameof(H_UpdateTimer))] public int timersListos = 0;        // Número de jugadores listos para iniciar el timer
    #endregion

    #endregion

    #region Variables Inicializables
    [Tooltip("Números iniciales de la partida")]
    [SerializeField] private Animator timerAnim;
    [Tooltip("Objeto semáforo de la escena")]
    [SerializeField] private GameObject semaphore;
    [Tooltip("Luces verdes del semáforo")]
    [SerializeField] private Material[] stateGreen;
    [Tooltip("Luces naranjas del semáforo")]
    [SerializeField] private Material[] stateOrange;
    [Tooltip("Luces rojas del semáforo")]
    [SerializeField] private Material[] stateRed;
    [Tooltip("Objeto controlador del timer inicial")]
    [SerializeField] private GameObject gameTimer;
    #endregion

    #region Variables Privadas
    private bool ended = false;                                   // Determina si la partida ha terminado
    private List<PlayerInfo> m_Players;                           // Lista de jugadores (copia de la lista del pole position manager)
    private float initialTime = 3;                                // Tiempo para que comience la carrera
    private SemaphoreSlim timerListo = new SemaphoreSlim(0);      // Semáforo que sincronizar el inicio del timer inicial
    private SemaphoreSlim jugadoresListos = new SemaphoreSlim(0); // Semáforo para sincronizar el final del timer inicial
    private bool calledToGameStarted = false;                     // Determina si se ha modificado el bool gameStarted
    private Text timerText;                                       // Texto del timer de la vuelta
    private Text totalTimerText;                                  // Texto del timer de la partida
    private PolePositionManager m_PPM;                            // Referencia al pole position manager
    private bool didStart = false;                                // Determina si comenzó la partida
    private bool auxBoolForServerOnly = false;                    // Determina si es Server Only
    #endregion

    /// <summary>
    /// Función a llamar por el delegado action.
    /// </summary>
    /// <param name="gs">Estado de la partida</param>
    void InvokeGameStarted(bool gs)
    {
        calledToGameStarted = gs;
    }

    #region Funciones Hook
    /// <summary>
    /// Inicia la animación del timer.
    /// </summary>
    /// <param name="anterior">Valor anterior</param>
    /// <param name="nuevo">Valor nuevo</param>
    void H_SetGameStarted(bool anterior, bool nuevo)
    {
        //m_PPM.m_PlayersNotOrdered = m_Players.ToList<PlayerInfo>();
        m_PPM.m_PlayersNotOrdered = m_Players.OrderBy(go => go.ID).ToList<PlayerInfo>(); //.OrderBy(go => go.ID);
        gameTimer.SetActive(true);
        timerAnim.SetTrigger("PlayTimer");
        foreach (PlayerInfo player in m_Players)
        {
            if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
            {
                int numPlayers = minPlayers;
                player.GetComponent<SetupPlayer>().CmdLapList(numPlayers);
            }

        }
    }

    /// <summary>
    /// Se realiza un Release de jugadoresListos si todos los jugadores están listos al terminar el timer inicial
    /// </summary>
    /// <param name="anterior">Valor Anterior</param>
    /// <param name="nuevo">Valor Nuevo</param>
    void H_UpdatePlayers(int anterior, int nuevo)
    {
        if (nuevo == minPlayers)
        {
            semaphore.GetComponent<MeshRenderer>().materials = stateGreen;
            jugadoresListos.Release(minPlayers);
        }
    }

    /// <summary>
    /// Se realiza un Release de jugadoresListos si todos los jugadores están listos para iniciar el timer
    /// </summary>
    /// <param name="anterior">Valor Anterior</param>
    /// <param name="nuevo">Valor Nuevo</param>
    void H_UpdateTimer(int anterior, int nuevo)
    {
        // if timersListos == numJugadores
        if (nuevo == minPlayers)
            timerListo.Release(1);
    }
    #endregion

    private void Start()
    {
        m_PPM = GetComponent<PolePositionManager>();
        timerText = GameObject.FindGameObjectWithTag("LapTimer").GetComponent<Text>();
        totalTimerText = GameObject.FindGameObjectWithTag("TotalTimer").GetComponent<Text>();
    }

    public void Update()
    {
        if (!gameStarted && !calledToGameStarted)
            return;
        else if (!gameStarted && calledToGameStarted)
        {
            foreach (PlayerInfo player in m_Players)
            {
                // Como no se puede hacer esta llamada desde un hilo, lo hacemos aquí
                if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
                    player.GetComponent<SetupPlayer>().CmdUpdateGameStarted();
            }
            return;
        }

        #region Temporizador Inicial
        if (initialTime > 0)
        {
            initialTime -= Time.deltaTime;
        }
        else
        {
            if (!ended)
            {

                PlayerInfo p = null; // Creamos un objeto auxiliar p
                int cont = 0;
                int cont2 = 0;
                int indice = 0;
                bool hayNulo = false;

                if (m_Players == null)
                {
                    m_Players = m_PolePositionManager.m_Players;
                }

                // Solo servidor
                if (isServerOnly)
                {
                    didStart = gameStarted;

                    if (didStart && !auxBoolForServerOnly)
                    {
                        auxBoolForServerOnly = true;
                        totalTimer.RestartTimer();
                    }
                }

                foreach (PlayerInfo player in m_Players)
                {
                    if (player)
                    {
                        if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
                            player.GetComponent<SetupPlayer>().CmdUpdatePlayers();

                        Thread endTimerThread = new Thread(() =>
                        {
                            jugadoresListos.Wait();
                            player.canMove = gameStarted;
                            lapTimer.RestartTimer();
                            totalTimer.RestartTimer();
                        });

                        endTimerThread.Start();
                    }
                    else
                    {
                        if (!hayNulo)
                        {
                            p = player; // p se iguala al player, es decir, a null porque se desconectó
                            cont2 = cont;
                        }
                        hayNulo = true;
                    }
                    cont++;
                }

                if (p == null && hayNulo) // si p es null significa que hay que eliminarlo de la lista
                {
                    for (int i = cont2; i < m_Players.Count - 1; i++)
                    {
                        m_Players[i] = m_Players[i + 1];
                        m_Players[i].CurrentPosition--;
                    }
                    indice = m_Players.Count - 1;

                    m_Players.RemoveAt(indice);
                    if (gameStarted)
                        return; // Volvemos a empezar el bucle, porque hay que comprobar si hay más players nulos

                }

                ended = true;
            }
            else
            {
                if (isServerOnly)
                {
                    totalTimer.CalculateTime();
                }

                PlayerInfo p = null;
                //p = new PlayerInfo();
                foreach (PlayerInfo player in m_Players)
                {
                    if (player)
                    {
                        if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
                        {
                            lapTimer.CalculateTime();
                            timerText.text = "Lap time: " + lapTimer.minutes + ":" + lapTimer.seconds + ":" + lapTimer.miliseconds;

                            totalTimer.CalculateTime();
                            totalTimerText.text = "Total time: " + totalTimer.minutes + ":" + totalTimer.seconds + ":" + totalTimer.miliseconds;
                        }
                    }
                    else
                    {
                        p = player;
                    }

                }
                if (p == null)
                {
                    m_Players.Remove(p);
                }

            }

        }

        if (initialTime < 2.55f && initialTime > 1.65f)
        {
            semaphore.GetComponent<MeshRenderer>().materials = stateRed;
        }
        else if (initialTime <= 1.65f && initialTime > 0.5f)
        {
            semaphore.GetComponent<MeshRenderer>().materials = stateOrange;
        }
        #endregion
    }

    /// <summary>
    /// Función llamada en el PolePositionManager, para comprobar si puede iniciarse la carrera
    /// </summary>
    /// <param name="numJugadores"></param>
    /// <param name="players"></param>
    public void UpdateGameStarted(int numJugadores, List<PlayerInfo> players)
    {
        // Se hace una copia de la lista
        m_Players = players.ToList<PlayerInfo>();

        // Se crea un delegado
        Action<bool> action = new Action<bool>(InvokeGameStarted);

        // Si el número de jugadores es el requerido para empezar la partida
        if (numJugadores >= minPlayers)
        {
            // Se actualiza el número de timers activos
            foreach (PlayerInfo player in m_Players)
            {
                if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
                    player.GetComponent<SetupPlayer>().CmdUpdateTimer();
            }

            // Se inicializa un hilo que congelamos
            Thread timerThread = new Thread(() =>
            {
                timerListo.Wait();   // Se espera a tener el permiso
                action.Invoke(true); // Llamada al delegado

            });

            // Se inicia el hilo
            timerThread.Start();
        }
    }
}
