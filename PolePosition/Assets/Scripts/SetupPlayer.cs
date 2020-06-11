using System;
using Mirror;
using UnityEngine;
using Random = System.Random;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

/// <summary>
/// Clase para configurar un jugador.
/// </summary>
public class SetupPlayer : NetworkBehaviour
{
    [SyncVar] private int m_ID; // ID del jugador
    [SyncVar(hook = nameof(SetName))] private string m_Name = "Player"; // Nombre del jugador
    [SyncVar(hook = nameof(SetCarType))] private int m_CarType = 0;     // Color del coche seleccionado
    //[SyncVar(hook = nameof())]

    private UIManager m_UIManager;                      // UIManager de la escena
    private NetworkManager m_NetworkManager;            // NetworkManager de la escena
    private PlayerController m_PlayerController;        // PlayerController del personaje (el vehículo)
    private PlayerInfo m_PlayerInfo;                    // Info del jugador
    private PolePositionManager m_PolePositionManager;  // Manager del juego

    //[SyncVar(hook = nameof(SetGameStarted))] bool gameStarted = false;

    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        m_ID = connectionToClient.connectionId;
    }

    #region Commands
    [Command]
    void CmdUpdateName(string name)
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        m_Name = name;
    }

    [Command]
    void CmdUpdateColor(int type)
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        m_CarType = type;
    }

    //[Command]
    //void CmdUpdateGameStarted(bool gs)
    //{
    //    GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
    //    gameStarted = gs;
    //}
    #endregion

    #region Funciones Hook
    void SetName(string anterior, string nuevo)
    {
        m_PlayerInfo.Name = nuevo; // Nombre del jugador
    }

    void SetCarType(int anterior, int nuevo)
    {
        m_PlayerInfo.SetCarType(nuevo);
        this.GetComponentInChildren<MeshRenderer>().materials = m_UIManager.cars[nuevo].carMaterials;
    }

    //void SetGameStarted(bool anterior, bool nuevo)
    //{
    //    gameStarted = nuevo;
    //}
    #endregion

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() // IMPORTANTE
    {
        base.OnStartClient();
        m_PlayerInfo.ID = m_ID; // ID del jugador
        #region De Germán

        if (isLocalPlayer)
        {
            m_Name = m_UIManager.playerName; // Nombre del jugador (variable privada)
            CmdUpdateName(m_Name);

            m_CarType = m_UIManager.carType;
            CmdUpdateColor(m_CarType);
            
            m_PlayerInfo.SetCarType(m_UIManager.carType);
        }

        Debug.Log("COLOR DE COCHE ESCOGIDO: <color=orange>" + m_PlayerInfo.carType + "</color>");
        #endregion
        m_PlayerInfo.CurrentLap = 0;                   // Vuelta actual alcanzada por el jugador

        m_PolePositionManager.AddPlayer(m_PlayerInfo); // Se añade el jugador a la lista de jugadores del manager de la partida

        //if (isLocalPlayer)
        //{
            //if (m_PolePositionManager.numPlayers > 1)
            //{
            //    gameStarted = true;
            //    Debug.Log("LA PARTIDA HA EMPEZADO");
            //    CmdUpdateGameStarted(gameStarted);
            //}
//}
    }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer()
    {
    }
    #endregion

    private void Awake()
    {
        m_PlayerInfo = GetComponent<PlayerInfo>();                       // Se busca el componente PlayerInfo
        m_PlayerController = GetComponent<PlayerController>();           // Se busca el componente PlayerController
        m_NetworkManager = FindObjectOfType<NetworkManager>();           // Se busca el NetworkManager
        m_PolePositionManager = FindObjectOfType<PolePositionManager>(); // Se busca el Manager del Pole Position
        m_UIManager = FindObjectOfType<UIManager>();                     // Se busca el Manager de la UI
    }

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            m_PlayerController.enabled = true;
            m_PlayerController.OnSpeedChangeEvent += OnSpeedChangeEventHandler;
            ConfigureCamera();
        }
    }

    void OnSpeedChangeEventHandler(float speed)
    {
        m_UIManager.UpdateSpeed((int) speed * 5); // 5 for visualization purpose (km/h)
    }

    void ConfigureCamera()
    {
        if (Camera.main != null)
        {
            Camera.main.gameObject.GetComponent<CameraController>().m_Focus = this.gameObject;
        }
    }
}