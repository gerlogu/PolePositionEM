using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mirror;
using UnityEngine;

/// <summary>
/// Manager que gestiona el estado de la partida.
/// </summary>
public class PolePositionManager : NetworkBehaviour
{
    #region Variables Públicas
    [Tooltip("Número de Jugadores")]
    [SyncVar(hook = nameof(H_UpdateNumPlayers))] public int numPlayers;
    [Tooltip("Network Manager")]
    public NetworkManager networkManager;

    public UIManager uiManager;

    public GameStartManager gameStartManager;
    #endregion

    #region Variables Privadas
    private readonly List<PlayerInfo> m_Players = new List<PlayerInfo>(4); // Lista de jugadores
    private CircuitController m_CircuitController; // Controlador del circuito
    public GameObject[] m_DebuggingSpheres;       // Esferas para depurar
    #endregion

    #region Funciones Hook
    void H_UpdateNumPlayers(int anterior, int nuevo)
    {
        numPlayers = nuevo;
    }
    #endregion

    #region Funciones Command
    [Command]
    void CmdUpdateNumPlayers(int n)
    {
        GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        numPlayers = n;
    }
    #endregion

    private void Awake()
    {
        // Si no existe network manager
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>(); // Se busca el networkManager en la escena
        }

        // Si no existe el controlador del circuito
        if (m_CircuitController == null)
        {
            m_CircuitController = FindObjectOfType<CircuitController>(); // Se busca el controlador del circuito en la escena
        }

        // Se inicializa el array de esferas en función del número de jugadores conectados
        m_DebuggingSpheres = new GameObject[networkManager.maxConnections];

        // Se inicializan las esferas
        for (int i = 0; i < networkManager.maxConnections; ++i)
        {
            m_DebuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            m_DebuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
        }
    }

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
        if (isLocalPlayer)
        {
            CmdUpdateNumPlayers(m_Players.Count);
        }

        uiManager.textNumPlayers.text = "P: " + m_Players.Count;
        gameStartManager.UpdateGameStarted(m_Players.Count, m_Players);
    }

    private class PlayerInfoComparer : Comparer<PlayerInfo>
    {
        float[] m_ArcLengths;

        public PlayerInfoComparer(float[] arcLengths)
        {
            m_ArcLengths = arcLengths;
        }

        public override int Compare(PlayerInfo x, PlayerInfo y)
        {
            if ((this.m_ArcLengths[x.ID] - m_ArcLengths[y.ID]) > float.Epsilon)
                return 1;
            else return -1;
        }
    }

    public void UpdateRaceProgress()
    {
        // Update car arc-lengths
        float[] arcLengths = new float[m_Players.Count];

        for (int i = 0; i < m_Players.Count; ++i)
        {
            arcLengths[i] = ComputeCarArcLength(i);
        }

        m_Players.Sort(new PlayerInfoComparer(arcLengths));

        string myRaceOrder = "";
        foreach (var _player in m_Players)
        {
            myRaceOrder += _player.Name + "\n";
        }
        uiManager.UpdatePlayerNames(myRaceOrder);
        Debug.Log("El orden de carrera es: " + myRaceOrder);
    }

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

        if (this.m_Players[ID].CurrentLap == 0)
        {
            minArcL -= m_CircuitController.CircuitLength;
        }
        else
        {
            minArcL += m_CircuitController.CircuitLength *
                       (m_Players[ID].CurrentLap - 1);
        }

        return minArcL;
    }
}