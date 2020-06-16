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
    public int minPlayers = 2;
    public LapTimer lapTimer = new LapTimer();
    public LapTimer totalTimer = new LapTimer();
    public PolePositionManager m_PolePositionManager;

    #region Variables Sincronizadas
    [HideInInspector] [SyncVar(hook = nameof(H_SetGameStarted))] public bool gameStarted = false; // Estado de la partida
    [HideInInspector] [SyncVar(hook = nameof(H_UpdatePlayers))] public int numPlayers = 0;        // Números de jugadores listos tras finalizar el timer
    [HideInInspector] [SyncVar(hook = nameof(H_UpdateTimer))] public int timersListos = 0;        // Número de jugadores listos para iniciar el timer
    #endregion

    #endregion

    #region Variables Inicializables
    [SerializeField] private Animator timerAnim;
    [SerializeField] private GameObject semaphore;
    [SerializeField] private Material[] stateGreen;
    [SerializeField] private Material[] stateOrange;
    [SerializeField] private Material[] stateRed;
    [SerializeField] private GameObject gameTimer;
    #endregion

    #region Variables Privadas
    private bool ended = false;
    private List<PlayerInfo> m_Players;
    private float timer = 3;
    private SemaphoreSlim timerListo = new SemaphoreSlim(0);
    private SemaphoreSlim jugadoresListos = new SemaphoreSlim(0);
    private bool calledToGameStarted = false;
    private Text timerText;
    private Text totalTimerText;
    private PolePositionManager m_PPM;
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
    /// Inicia la animación del timer
    /// </summary>
    /// <param name="anterior">Valor anterior</param>
    /// <param name="nuevo">Valor nuevo</param>
    void H_SetGameStarted(bool anterior, bool nuevo)
    {
        gameTimer.SetActive(true);
        timerAnim.SetTrigger("PlayTimer");
        Debug.Log("NÚMERO DE JUGADORES (TAMAÑO DE LA LISTA): <color=orange>" + m_PolePositionManager.playersArcLengths.Count + "</color>");
    }

    /// <summary>
    /// Se realiza un Release de jugadoresListos si todos los jugadores están listos al terminar el timer inicial
    /// </summary>
    /// <param name="anterior">Valor Anterior</param>
    /// <param name="nuevo">Valor Nuevo</param>
    void H_UpdatePlayers(int anterior, int nuevo)
    {
        if(nuevo == minPlayers)
        {
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
        if(nuevo == minPlayers)
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
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            
            if (!ended)
            {
                foreach (PlayerInfo player in m_Players)
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
                ended = true;
            }
            else
            {
                foreach (PlayerInfo player in m_Players)
                {
                    if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
                    {
                        if (!player.hasFinished)
                        {
                            lapTimer.CalculateTime();
                            timerText.text = "Lap time: " + lapTimer.minutes + ":" + lapTimer.seconds + ":" + lapTimer.miliseconds;

                            totalTimer.CalculateTime();
                            totalTimerText.text = "Total time: " + totalTimer.minutes + ":" + totalTimer.seconds + ":" + totalTimer.miliseconds;
                        }
                    }
                }
                
            }

        }

        if (timer < 2.55f && timer > 1.65f)
        {
            semaphore.GetComponent<MeshRenderer>().materials = stateRed;
        }
        else if (timer <= 1.65f && timer > 0.5f)
        {
            semaphore.GetComponent<MeshRenderer>().materials = stateOrange;
        }
        else if (timer <= 0.5f)
        {
            semaphore.GetComponent<MeshRenderer>().materials = stateGreen;
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
