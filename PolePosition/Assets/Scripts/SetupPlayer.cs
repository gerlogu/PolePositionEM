﻿using System;
using Mirror;
using Mirror.Examples.Basic;
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
    [SyncVar(hook = nameof(SetID))] private int m_ID;                   // ID del jugador
    [SyncVar(hook = nameof(SetName))] private string m_Name = "Player"; // Nombre del jugador
    [SyncVar(hook = nameof(SetCarType))] private int m_CarType = 0;     // Color del coche seleccionado
    [SyncVar] private bool thereIsServerOnly = false;
    

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

        if (!isServerOnly)
        {
            m_ID = connectionToClient.connectionId - m_PolePositionManager.numDesconexiones;
        }
        else
        {
            m_ID = connectionToClient.connectionId - 1 - m_PolePositionManager.numDesconexiones;

            m_PlayerInfo.CurrentLap = 0;                   // Vuelta actual alcanzada por el jugador

            // No sense pero lo dejamos a modo de comentario
            //if (isLocalPlayer)
            //{
            //    CmdUpdateName(m_UIManager.playerName);

            //    CmdUpdateColor(m_UIManager.carType);
            //    Debug.Log("Nombre del jugador:" + m_LapController.m_playerInfo.Name);

            //    string carColor = m_PlayerInfo.carType.ToString();
            //    Debug.Log("COLOR DE COCHE ESCOGIDO: <color=" + carColor + ">" + m_PlayerInfo.carType + "</color>");
            //}

            Debug.Log("ID del coche: " + m_PlayerInfo.ID);

            m_PolePositionManager.AddPlayer(m_PlayerInfo); // Se añade el jugador a la lista de jugadores del manager de la partida

        }

        if (!isServerOnly)
        {
            m_PlayerInfo.ID = m_ID;
            //m_ID = connectionToClient.connectionId;
            thereIsServerOnly = false;
        }
        else if (!thereIsServerOnly)
        {
            m_PlayerInfo.ID = m_ID;
            //m_ID = connectionToClient.connectionId;
        }
        else if (thereIsServerOnly)
        {
            m_PlayerInfo.ID = m_ID - 1;
            //m_ID = connectionToClient.connectionId - 1;
        }
    }

    void H_UpdateNumDisconnections(int before, int after)
    {
        m_PolePositionManager.numDesconexiones = after;
    }

    #region Commands
    [Command]
    void CmdUpdateName(string name, int ID)
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        m_Name = name;
        PolePositionManager ppm = FindObjectOfType<PolePositionManager>();

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

    [Command]
    void CmdUpdateColor(int type)
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        m_CarType = type;
    }

    [Command]
    public void CmdUpdateNumDisconnections()
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        m_PolePositionManager.numDesconexiones++;
    }

    #region Commands del GameStartManager
    [Command]
    public void CmdUpdatePlayers()
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        m_PolePositionManager.m_GameStartManager.numPlayers++; // Se actualiza el número de jugadores LISTOS tras terminar el timer
    }

    [Command]
    public void CmdUpdateTimer()
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        m_PolePositionManager.m_GameStartManager.timersListos++; // Se actualiza el número de timers listos para iniciarse
    }

    /// <summary>
    /// Se actualiza el estado de la partida a empezada.
    /// </summary>
    [Command]
    public void CmdUpdateGameStarted()
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        m_PolePositionManager.m_GameStartManager.gameStarted = true; // Se actualiza el estado de la partida para los jugadores
    }

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

    [Command]
    public void CmdUpdateLaps(int pos, int laps, int playerID)
    {
        //GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
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
    void SetName(string anterior, string nuevo)
    {
        m_PlayerInfo.Name = nuevo; // Nombre del jugador
        GetComponentInChildren<TextMesh>().text = nuevo;
        this.name = nuevo;
    }

    void SetCarType(int anterior, int nuevo)
    {
        m_PlayerInfo.SetCarType(nuevo);
        this.GetComponentInChildren<MeshRenderer>().materials = m_UIManager.cars[nuevo].carMaterials;
    }

    void SetID(int anterior, int nuevo)
    {
        m_PlayerInfo.ID = nuevo;
    }
    #endregion

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() // IMPORTANTE
    {
        base.OnStartClient();

        //numDesconexiones = CmdGetNumDisconnections();

        if ((m_ID - m_PolePositionManager.numDesconexiones)> m_PolePositionManager.m_GameStartManager.minPlayers - 1)
        {
            //numDesconexiones++;
            CmdUpdateNumDisconnections();
            Debug.Log("Capacidad superada, desconectando al jugador");
            if (isLocalPlayer)
            {
                m_UIManager = FindObjectOfType<UIManager>();
                m_UIManager.ShowConnectionErrorMessage();
            }
            //connectionToClient.Dispose();
            connectionToClient.Disconnect();
            return;
        }

        if (!isServerOnly)
        {
            m_PlayerInfo.CurrentLap = 0;                   // Vuelta actual alcanzada por el jugador

            if (isLocalPlayer)
            {
                CmdUpdateName(m_UIManager.playerName, m_ID);
                CmdUpdateColor(m_UIManager.carType);
            }

            Debug.Log("ID del coche: " + m_PlayerInfo.ID);

            m_PolePositionManager.AddPlayer(m_PlayerInfo); // Se añade el jugador a la lista de jugadores del manager de la partida
        }
        else
        {

        }
        
    }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        #region Player Components
        m_PlayerController.enabled  = true;
        m_CrashDetector.enabled     = true;
        m_DirectionDetector.enabled = true;
        m_LapController.enabled     = true;
        
        m_PlayerController.OnSpeedChangeEvent += OnSpeedChangeEventHandler;
        ConfigureCamera();
        #endregion
    }
    #endregion

    private void Awake()
    {
        m_PlayerInfo          = GetComponent<PlayerInfo>();              // Se busca el componente PlayerInfo
        m_PlayerController    = GetComponent<PlayerController>();        // Se busca el componente PlayerController
        m_NetworkManager      = FindObjectOfType<NetworkManager>();      // Se busca el NetworkManager
        m_PolePositionManager = FindObjectOfType<PolePositionManager>(); // Se busca el Manager del Pole Position
        m_UIManager           = FindObjectOfType<UIManager>();           // Se busca el Manager de la UI
        m_CrashDetector       = GetComponent<CrashDetector>();           // Se busca el componente CrashDetector
        m_DirectionDetector   = GetComponent<DirectionDetector>();       // Se busca el componente DirectionDetector
        m_LapController       = GetComponent<LapController>();           // Se buesca el componente LapController
        m_LapManager          = FindObjectOfType<LapManager>();          // Se busca el Manager de las vueltas
    }

    // Start is called before the first frame update
    void Start()
    {
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