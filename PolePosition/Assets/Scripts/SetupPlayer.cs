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
    [SyncVar] private int m_ID;                         // ID del jugador
    [SyncVar] private string m_Name = "Player";         // Nombre del jugador

    private UIManager m_UIManager;                      // UIManager de la escena
    private NetworkManager m_NetworkManager;            // NetworkManager de la escena
    private PlayerController m_PlayerController;        // PlayerController del personaje (el vehículo)
    private PlayerInfo m_PlayerInfo;                    // Info del jugador
    private PolePositionManager m_PolePositionManager;  // Manager del juego

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

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() // IMPORTANTE
    {
        base.OnStartClient();
        m_PlayerInfo.ID = m_ID; // ID del jugador
        #region De Germán
        m_Name = m_UIManager.playerName; // Nombre del jugador (variable privada)
        m_PlayerInfo.Name = m_Name;      // Nombre del jugador
        this.GetComponentInChildren<TextMesh>().text = m_PlayerInfo.Name;
        if (isLocalPlayer)
        {
            this.GetComponentInChildren<MeshRenderer>().materials = m_NetworkManager.playerCarMaterials;
            m_PlayerInfo.SetCarType(m_NetworkManager.carType);
        }
        Debug.Log("COLOR DE COCHE ESCOGIDO: <color=orange>" + m_PlayerInfo.carType + "</color>");
        #endregion
        m_PlayerInfo.CurrentLap = 0;                   // Vuelta actual alcanzada por el jugador
        m_PolePositionManager.AddPlayer(m_PlayerInfo); // Se añade el jugador a la lista de jugadores del manager de la partida
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