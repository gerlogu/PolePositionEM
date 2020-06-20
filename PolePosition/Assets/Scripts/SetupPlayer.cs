using Mirror;
using UnityEngine;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

/// <summary>
/// Clase para configurar un jugador.
/// </summary>
public class SetupPlayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(SetID))] private int m_ID;                   // ID del jugador
    [SyncVar(hook = nameof(SetName))] private string m_Name = "Player"; // Nombre del jugador
    [SyncVar(hook = nameof(SetCarType))] private int m_CarType = 0;     // Color del coche seleccionado
    [SyncVar] private bool thereIsServerOnly = false;                   // Booleano que indica si se está ejecutando el modo Server Only

    private UIManager m_UIManager;                      // UIManager de la escena
    private NetworkManager m_NetworkManager;            // NetworkManager de la escena
    private PlayerController m_PlayerController;        // PlayerController del personaje (el vehículo)
    private PlayerInfo m_PlayerInfo;                    // Info del jugador
    private PolePositionManager m_PolePositionManager;  // Manager del juego
    private CrashDetector m_CrashDetector;              // Detector de colisiones
    private DirectionDetector m_DirectionDetector;      // Detector de dirección
    private LapController m_LapController;              // Controlador de vueltas
    public LapManager m_LapManager;

    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();

        // Si no es solo servidor
        if (!isServerOnly)
        {

            if(m_PolePositionManager.disconnectedPlayerIds.Count > 0)
            {
                m_ID = m_PolePositionManager.disconnectedPlayerIds[0];
                m_PolePositionManager.disconnectedPlayerIds.RemoveAt(0);
            }
            else
            {
                // Se obtiene la ID de la conexion
                m_ID = connectionToClient.connectionId - m_PolePositionManager.numDesconexiones;
            }

            this.transform.position = m_PolePositionManager.Spawns[m_ID].position;

        }
        // Si iniciamos el modo solo servidor
        else
        {
            if (m_PolePositionManager.disconnectedPlayerIds.Count > 0)
            {
                m_ID = m_PolePositionManager.disconnectedPlayerIds[0];
                m_PolePositionManager.disconnectedPlayerIds.RemoveAt(0);
            }
            else
            {
                // Se asigna como ID la id - 1
                m_ID = connectionToClient.connectionId - 1 - m_PolePositionManager.numDesconexiones;
            }

            this.transform.position = m_PolePositionManager.Spawns[m_ID].position;

            m_PlayerInfo.CurrentLap = 0; // Vuelta actual alcanzada por el jugador

            m_PolePositionManager.AddPlayer(m_PlayerInfo); // Se añade el jugador a la lista de jugadores del manager de la partida
        }

        // Si no es solo servidor
        if (!isServerOnly)
        {
            // Asignamos la id al coche
            m_PlayerInfo.ID = m_ID;
            thereIsServerOnly = false;
        }
        // Si es solo servidor
        else if (!thereIsServerOnly)
        {
            // Se asigna la ID normal si no es el servidor
            m_PlayerInfo.ID = m_ID;
        }
        else if (thereIsServerOnly)
        {
            // Se asigna la ID -1 al servidor
            m_PlayerInfo.ID = m_ID - 1;
        }
    }

    #region Commands

    /// <summary>
    /// Funcion que asigna el nombre al jugador correspondiente
    /// </summary>
    /// <param name="name">Nombre del jugador</param>
    /// <param name="ID">ID del jugador</param>
    [Command]
    void CmdUpdateName(string name, int ID)
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        m_Name = name;
        PolePositionManager ppm = FindObjectOfType<PolePositionManager>();

        // Se asigna el nombre al jugador correspondiente
        switch (ID)
        {
            case 0:
                ppm.player1Name = name;
                break;
            case 1:
                ppm.player2Name = name;
                break;
            case 2:
                ppm.player3Name = name;
                break;
            case 3:
                ppm.player4Name = name;
                break;
        }
    }

    /// <summary>
    /// Funcion que actualiza el color del coche
    /// </summary>
    /// <param name="type">color del coche</param>
    [Command]
    void CmdUpdateColor(int type)
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        m_CarType = type;
    }

    /// <summary>
    /// Funcion que actualiza el numero de desconexiones
    /// </summary>
    [Command]
    public void CmdUpdateNumDisconnections(int id)
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        m_PolePositionManager.numDesconexiones++;
        m_PolePositionManager.disconnectedPlayerIds.Add(id);
    }

    #region Commands del GameStartManager

    /// <summary>
    /// Funcion que actualiza el numero de jugadores
    /// </summary>
    [Command]
    public void CmdUpdatePlayers()
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        m_PolePositionManager.m_GameStartManager.numPlayers++; // Se actualiza el número de jugadores LISTOS tras terminar el timer
    }

    /// <summary>
    /// Funcion que actualiza el numero de timers listos para iniciarse
    /// </summary>
    [Command]
    public void CmdUpdateTimer()
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        m_PolePositionManager.m_GameStartManager.timersListos++; // Se actualiza el número de timers listos para iniciarse
    }

    /// <summary>
    /// Se actualiza el estado de la partida a empezada
    /// </summary>
    [Command]
    public void CmdUpdateGameStarted()
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        m_PolePositionManager.m_GameStartManager.gameStarted = true; // Se actualiza el estado de la partida para los jugadores
    }

    /// <summary>
    /// Funcion que inicializa el numero de vueltas de los jugadores a 0
    /// </summary>
    /// <param name="numPlayers">numero de jugadores</param>
    [Command]
    public void CmdLapList(int numPlayers)
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        switch (numPlayers)
        {
            case 1:
                m_LapManager.player1Laps = 0;
                break;
            case 2:
                m_LapManager.player1Laps = 0;
                m_LapManager.player2Laps = 0;
                break;
            case 3:
                m_LapManager.player1Laps = 0;
                m_LapManager.player2Laps = 0;
                m_LapManager.player3Laps = 0;
                break;
            case 4:
                m_LapManager.player1Laps = 0;
                m_LapManager.player2Laps = 0;
                m_LapManager.player3Laps = 0;
                m_LapManager.player4Laps = 0;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Funcion que actualiza el numero de vueltas de un jugador
    /// </summary>
    /// <param name="laps">Numero de vueltas</param>
    /// <param name="playerID">Identificador del jugador</param>
    [Command]
    public void CmdUpdateLaps(int laps, int playerID)
    {
        lock (m_PolePositionManager.xLock)
        {
            switch (playerID)
            {
                case 0:
                    m_LapManager.player1Laps = laps;
                    break;
                case 1:
                    m_LapManager.player2Laps = laps;
                    break;
                case 2:
                    m_LapManager.player3Laps = laps;
                    break;
                case 3:
                    m_LapManager.player4Laps = laps;
                    break;
            }
        }
    }
    #endregion

    #endregion

    #region Funciones Hook

    /// <summary>
    /// Funcion que establece el nombre del jugador
    /// </summary>
    /// <param name="anterior">Cadena anterior</param>
    /// <param name="nuevo">Cadena posterior</param>
    void SetName(string anterior, string nuevo)
    {
        m_PlayerInfo.Name = nuevo; // Nombre del jugador
        GetComponentInChildren<TextMesh>().text = nuevo;
        this.name = nuevo;
    }

    /// <summary>
    /// Funcion que establece el color del coche
    /// </summary>
    /// <param name="anterior">Color anterior</param>
    /// <param name="nuevo">Color posterior</param>
    void SetCarType(int anterior, int nuevo)
    {
        m_PlayerInfo.SetCarType(nuevo);
        this.GetComponentInChildren<MeshRenderer>().materials = m_UIManager.cars[nuevo].carMaterials;
    }

    /// <summary>
    /// Funcion que establece la ID del jugador
    /// </summary>
    /// <param name="anterior">Identificador anterior</param>
    /// <param name="nuevo">Identificador posterior</param>
    void SetID(int anterior, int nuevo)
    {
        m_PlayerInfo.ID = nuevo;
    }
    #endregion

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();

        Debug.Log("OnStartClient, " + m_ID);
        // Gestion de desconexiones
        if ((m_ID - m_PolePositionManager.numDesconexiones) > m_PolePositionManager.m_GameStartManager.minPlayers - 1)
        {
            Debug.Log("Entro en el primer IF");
            if (isLocalPlayer)
            {
                Debug.Log("IsLocalPlayer");
                m_UIManager = FindObjectOfType<UIManager>();
                m_UIManager.ShowConnectionErrorMessage();
            }

            connectionToClient.Disconnect(); // ESTO ES LO QUE FALLA
            return;
        }

        this.transform.position = m_PolePositionManager.Spawns[m_ID].position;

        // Si no es solo servidor
        if (!isServerOnly)
        {
            m_PlayerInfo.CurrentLap = 0; // Vuelta actual alcanzada por el jugador

            // Si es el jugador local
            if (isLocalPlayer)
            {
                // Se actualiza nombre y color
                CmdUpdateName(m_UIManager.playerName, m_ID);
                CmdUpdateColor(m_UIManager.carType);
            }

            m_PolePositionManager.AddPlayer(m_PlayerInfo); // Se añade el jugador a la lista de jugadores del manager de la partida
        }
    }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        #region Player Components
        // Se activan los componentes necesarios
        m_PlayerController.enabled  = true;
        m_CrashDetector.enabled     = true;
        m_DirectionDetector.enabled = true;
        m_LapController.enabled     = true;
        
        // Se asigna la funcion al evento de delegado (realmente estamos asignando el evento al handler,
        // pero el nombre de la variable no es explicativo)
        m_PlayerController.OnSpeedChangeEvent += OnSpeedChangeEventHandler;

        // Se configura la camara
        ConfigureCamera();
        #endregion
    }
    #endregion

    #region Unity Callbacks
    /// <summary>
    /// Funcion Awake, que inicializa las siguientes variables
    /// </summary>
    private void Awake()
    {
        m_PlayerInfo          = GetComponent<PlayerInfo>();              // Se busca el componente PlayerInfo
        m_PlayerController    = GetComponent<PlayerController>();        // Se busca el componente PlayerController
        m_NetworkManager      = FindObjectOfType<NetworkManager>();      // Se busca el NetworkManager
        m_PolePositionManager = FindObjectOfType<PolePositionManager>(); // Se busca el Manager del Pole Position
        m_UIManager           = FindObjectOfType<UIManager>();           // Se busca el Manager de la UI
        m_CrashDetector       = GetComponent<CrashDetector>();           // Se busca el componente CrashDetector
        m_DirectionDetector   = GetComponent<DirectionDetector>();       // Se busca el componente DirectionDetector
        m_LapController       = GetComponent<LapController>();           // Se busca el componente LapController
        m_LapManager          = FindObjectOfType<LapManager>();          // Se busca el Manager de las vueltas
    }

    #endregion
    
    /// <summary>
    /// Funcion que se llama por el delegado cuando cambia la velocidad
    /// </summary>
    /// <param name="speed">Velocidad</param>
    void OnSpeedChangeEventHandler(float speed)
    {
        m_UIManager.UpdateSpeed((int) speed * 5); // 5 for visualization purpose (km/h)
    }

    /// <summary>
    /// Funcion que configura la camara
    /// </summary>
    void ConfigureCamera()
    {
        if (Camera.main != null)
        {
            Camera.main.gameObject.GetComponent<CameraController>().m_Focus = this.gameObject;
        }
    }
}