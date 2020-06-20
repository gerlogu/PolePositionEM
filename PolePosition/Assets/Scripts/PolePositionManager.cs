using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// Manager que gestiona el estado de la partida.
/// </summary>
public class PolePositionManager : NetworkBehaviour
{
    #region Variables Públicas
    [SerializeField] private bool debugSpheres = false;

    [Header("Synchronized Attributes")]
    [Tooltip("Número de Jugadores")]
    [SyncVar] public int numPlayers;
    [Tooltip("Número de desconexiones totales")]
    [SyncVar] public int numDesconexiones;
    [Tooltip("Bool que determina si la partida ha terminado")]
    [SyncVar] public bool gameHasEnded;
    [Tooltip("Nombre del jugador 1")]
    [SyncVar] public string player1Name;
    [Tooltip("Nombre del jugador 2")]
    [SyncVar] public string player2Name;
    [Tooltip("Nombre del jugador 3")]
    [SyncVar] public string player3Name;
    [Tooltip("Nombre del jugador 4")]
    [SyncVar] public string player4Name;
    public SyncListInt disconnectedPlayerIds = new SyncListInt();

    [Tooltip("Lista de jugadores ordenados por posicion")] public List<PlayerInfo> m_Players = new List<PlayerInfo>(4);
    [Tooltip("Lista de jugadores ordenados por ID")] public List<PlayerInfo> m_PlayersNotOrdered = new List<PlayerInfo>(4);
    [Tooltip("Esferas de debug")] public GameObject[] m_DebuggingSpheres;
    [Tooltip("Longitudes de arco")] public float[] m_arcLengths;

    [Header("References")]
    [Tooltip("Network Manager")]
    public NetworkManager m_NetworkManager;
    [Tooltip("Referencia al UIManager de la escena")]
    public UIManager m_UIManager;
    [Tooltip("Referencia del GameStartManager de la escena")]
    public GameStartManager m_GameStartManager;
    [Tooltip("Referencia del LapManager de la escena")]
    public LapManager m_LapManager;

    [Header("Others")]
    [Tooltip("Cerrojo principal")]
    public object xLock = new object();
    #endregion

    #region Variables Privadas
    private CircuitController m_CircuitController;    // Controlador del circuito
    private bool someoneFinished = false;             // Bool que determina si algún jugador ha completado la carrera
    private bool timeEnded = false;                   // Bool que determina si el temporizador de final de partida ha concluido
    private float timeToEnd = 20.0f;                  // Tiempo que dura el temporizador de final de partida
    private bool hasPlayerNames = false;              // Bool que determina si se conoce el nombre de todos los jugadores      
    #endregion

    /// <summary>
    /// Método Awake
    /// </summary>
    private void Awake()
    {
        // Si no existe network manager
        if (!m_NetworkManager)
        {
            m_NetworkManager = FindObjectOfType<NetworkManager>(); // Se busca el networkManager en la escena
        }

        // Si no existe el controlador del circuito
        if (!m_CircuitController)
        {
            m_CircuitController = FindObjectOfType<CircuitController>(); // Se busca el controlador del circuito en la escena
        }

        // Si no existe el game start manager
        if (!m_GameStartManager)
        {
            m_GameStartManager = GetComponent<GameStartManager>(); // Se busca el componente en el objeto
        }

        // Se inicializa el array de esferas en función del número de jugadores conectados
        m_DebuggingSpheres = new GameObject[m_NetworkManager.maxConnections];

        // Se inicializan las esferas
        for (int i = 0; i < m_NetworkManager.maxConnections; ++i)
        {
            m_DebuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            m_DebuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
            if (!debugSpheres)
            {
                m_DebuggingSpheres[i].GetComponent<MeshRenderer>().enabled = false;
            }
        }

        // Se inicializa a 20 segundos
        timeToEnd = 20.0f;
    }

    /// <summary>
    /// Función Update, que se ejecuta cada frame
    /// </summary>
    private void Update()
    {
        // Si el número de jugadores es igual a cero, no se actualiza el progreso de la carrera
        if (m_Players.Count == 0)
            return;

        UpdateRaceProgress(); // Se actualiza el progreso de la carrera
    }

    /// <summary>
    /// Añade un jugador a la lista de jugadores
    /// </summary>
    /// <param name="player">Jugador nuevo</param>
    public void AddPlayer(PlayerInfo player)
    {
        m_Players.Add(player); // Se añade a la lista el jugador
        m_PlayersNotOrdered.Add(player); // Se añade jugador a la lista no ordenada
        m_UIManager.textNumPlayers.text = "P: " + m_Players.Count; // Se actualiza el texto que muestra el número de jugadores
        
        // Si existe Game Start Manager
        if(m_GameStartManager)
            m_GameStartManager.UpdateGameStarted(m_Players.Count, m_Players); // Se actualiza el número de jugadores del Game Start Manager
    }

    /// <summary>
    /// Comparador de la posición de los jugadores.
    /// </summary>
    private class PlayerInfoComparer : Comparer<PlayerInfo>
    {
        float[] m_ArcLengths; // Longitudes de arco de las esferas de los jugadores

        /// <summary>
        /// Constructor de la clase
        /// </summary>
        /// <param name="arcLengths">Logitudes de arco actuales</param>
        public PlayerInfoComparer(float[] arcLengths)
        {
            m_ArcLengths = arcLengths;
        }

        /// <summary>
        /// Comparador de la posición de dos coches para saber quién va por delante del otro.
        /// </summary>
        /// <param name="x">Primer jugador</param>
        /// <param name="y">Segundo jugador</param>
        /// <returns>Valor que determina cuál va primero</returns>
        public override int Compare(PlayerInfo x, PlayerInfo y)
        {
            var diferencia = m_ArcLengths[x.CurrentPosition] - m_ArcLengths[y.CurrentPosition];
            if (diferencia < -float.Epsilon)
            {
                return 1;
            }
            else return -1;
        }
    }


    /// <summary>
    /// Actualiza el estado de la carrera
    /// </summary>
    public void UpdateRaceProgress()
    {
        // Lock para que esto no se solape con otros procesos
        lock (xLock)
        {
            // Eliminación de jugadores desconectados
            PlayerInfo p = null; // Creamos un objeto auxiliar p
            bool hayNulo = false;
            int cont = 0;
            int cont2 = 0;
            int indice = 0;

            foreach (PlayerInfo player in m_Players) // Recorremos la lista de jugadores
            {
                if (player == null) // Si el player es null (es decir, si se ha desconectado)
                {
                    m_PlayersNotOrdered.Remove(player); // Eliminamos el jugador de la lista sin ordenar
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

                foreach (PlayerInfo pj in m_Players)
                {
                    if (pj.GetComponent<SetupPlayer>().isServer)
                    {
                        if (NetworkClient.active)
                        {
                            Debug.LogWarning("JUGADOR DESCONECTADO | ID: " + cont2);
                            pj.GetComponent<SetupPlayer>().CmdUpdateNumDisconnections(cont2);
                        }
                        else
                        {
                            Debug.LogWarning("Intento de desconexión interrumpido por finalización de la partida, el servidor será desconectado.");
                        }
                    }
                }

                if (m_GameStartManager.gameStarted)
                    return; // Volvemos a empezar el bucle, porque hay que comprobar si hay más players nulos
            }


            // Se actualizan las longitudes de arco
            m_arcLengths = new float[m_Players.Count];

            // Por cada jugador
            for (int i = 0; i < m_Players.Count; ++i)
            {
                // Se actualiza la posición de la esfera si el personaje se encuentra dentro del camino a seguir
                if (m_Players[i].GetComponent<CrashDetector>().isGrounded)
                {
                    m_arcLengths[i] = ComputeCarArcLength(i);
                }
            }

            // Se ordena la lista por longitudes de arco (posicion)
            m_Players.Sort(new PlayerInfoComparer(m_arcLengths));

            // Se asigna la posicion a cada jugador
            for (int i = 0; i < m_Players.Count; ++i)
            {
                m_Players[i].CurrentPosition = i;
            }

            // Calculos del servidor
            if (isServerOnly)
            {
                // Se obtiene una referencia al LapManager
                LapManager m_lapManager = FindObjectOfType<LapManager>();

                // Se guardan las vueltas de cada jugador y se comprueba si han acabado
                int[] playerLaps = { m_lapManager.player1Laps, m_lapManager.player2Laps, m_lapManager.player3Laps, m_lapManager.player4Laps };

                for (int i = 0; i < m_PlayersNotOrdered.Count; i++)
                {
                    m_PlayersNotOrdered[i].CurrentLap = playerLaps[i];
                    switch (i)
                    {
                        case 0:
                            m_PlayersNotOrdered[i].hasFinished = m_LapManager.player1Finished;
                            break;
                        case 1:
                            m_PlayersNotOrdered[i].hasFinished = m_LapManager.player2Finished;
                            break;
                        case 2:
                            m_PlayersNotOrdered[i].hasFinished = m_LapManager.player3Finished;
                            break;
                        case 3:
                            m_PlayersNotOrdered[i].hasFinished = m_LapManager.player4Finished;
                            break;
                    }

                    if (m_PlayersNotOrdered[i].hasFinished)
                    {
                        someoneFinished = true;
                    }
                }

                // Si alguien ha terminado
                if (someoneFinished)
                {
                    // Por cada jugador miramos si este ha terminado para guardar su tiempo total
                    foreach (PlayerInfo player in m_Players)
                    {
                        if (player.hasFinished)
                        {
                            string[] totalTimes = new string[3];

                            switch (player.ID)
                            {
                                case 0:
                                    totalTimes = m_lapManager.player1TotalTimer.Split(':');
                                    break;
                                case 1:
                                    totalTimes = m_lapManager.player2TotalTimer.Split(':');
                                    break;
                                case 2:
                                    totalTimes = m_lapManager.player3TotalTimer.Split(':');
                                    break;
                                case 3:
                                    totalTimes = m_lapManager.player4TotalTimer.Split(':');
                                    break;
                            }

                            player.lapTotalMinutes = totalTimes[0];
                            player.lapTotalSeconds = totalTimes[1];
                            player.lapTotalMiliseconds = totalTimes[2];
                        }
                    }

                    // Se actualiza el tiempo para terminar la partida
                    timeToEnd -= Time.deltaTime;
                    m_lapManager.timeToEnd = timeToEnd;
                    GetComponent<FinishGame>().RpcUpdateEndTimerText(Mathf.RoundToInt(m_lapManager.timeToEnd));

                    // Si el tiempo ha acabado, se actualiza la variable que lo controla y se muestra la informacion de final de partida
                    if (m_LapManager.timeToEnd <= 0 && !timeEnded)
                    {
                        timeEnded = true;
                        GetComponent<FinishGame>().RpcUpdateReadyToShow();
                    }
                }

                // Si no tiene los nombres de los jugadores y la partida ha empezado, los obtiene
                if (!hasPlayerNames && m_GameStartManager.gameStarted)
                {
                    foreach (PlayerInfo player in m_PlayersNotOrdered)
                    {
                        switch (player.ID)
                        {
                            case 0:
                                player.Name = player1Name;
                                break;
                            case 1:
                                player.Name = player2Name;
                                break;
                            case 2:
                                player.Name = player3Name;
                                break;
                            case 3:
                                player.Name = player4Name;
                                break;
                        }
                    }
                    hasPlayerNames = true;
                }
            }
        }

        // Se actualiza en la interfaz la posicion de cada jugador
        string myRaceOrder = "";
        foreach (var _player in m_Players)
        {
            myRaceOrder += _player.Name + "\n";
        }
        m_UIManager.UpdatePlayerNames(myRaceOrder);
    }

    /// <summary>
    /// Cálculo de las longitudes de arco en función de la ID del jugador
    /// </summary>
    /// <param name="ID">Identificador del jugador (de 0 a 3)</param>
    /// <returns></returns>
    float ComputeCarArcLength(int ID)
    {
        // Compute the projection of the car position to the closest circuit 
        // path segment and accumulate the arc-length along of the car along
        // the circuit.
        Vector3 carPos = this.m_Players[ID].transform.position;

        int segIdx;
        float carDist;
        Vector3 carProj;

        float minArcL =
            this.m_CircuitController.ComputeClosestPointArcLength(carPos, out segIdx, out carProj, out carDist);

        this.m_DebuggingSpheres[ID].transform.position = carProj;

        if (m_GameStartManager.gameStarted)
        {
            if (m_Players[ID].CurrentLap == 0)
            {
                minArcL -= m_CircuitController.CircuitLength;
            }
            else
            {
                minArcL += m_CircuitController.CircuitLength *
                                   (m_Players[ID].CurrentLap - 1);
            }
        }
        return minArcL;
    }
}