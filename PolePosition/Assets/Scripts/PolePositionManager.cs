using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirror;
using UnityEngine;

using System.Threading.Tasks;

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

    public object xLock = new object();

    [SyncVar] public bool gameHasEnded;

    //public SyncListFloat playersArcLengths;

    public LapManager m_LapManager;
    #endregion

    #region Variables Privadas
    public List<PlayerInfo> m_Players = new List<PlayerInfo>(4); // Lista de jugadores
    public List<PlayerInfo> m_PlayersNotOrdered = new List<PlayerInfo>(4); // Lista de jugadores
    private CircuitController m_CircuitController;                         // Controlador del circuito
    public GameObject[] m_DebuggingSpheres;                                // Esferas para depurar
    public float[] m_arcLengths;                                           // Longitudes de arco
    #endregion

    #region Funciones Hook
    void H_UpdateNumPlayers(int anterior, int nuevo)
    {
        numPlayers = nuevo;
    }
    #endregion

    #region Funciones Command
    //[Command]
    //void CmdUpdateGameReady(int n)
    //{
    //    GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
    //    numPlayers = n;
    //}
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
        //CmdUpdateGameReady(m_Players.Count);
        uiManager.textNumPlayers.text = "P: " + m_Players.Count;
        if(gameStartManager)
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
            var diferencia = m_ArcLengths[x.CurrentPosition] - m_ArcLengths[y.CurrentPosition];
            if (diferencia < -float.Epsilon)
            {
                return 1;
            }
            else return -1;
        }
    }

    public void UpdateRaceProgress()
    {

        // Lock para que esto no se solape con otros procesos
        lock (xLock)
        {
            // Esto no puede fallar, lo hago así, porque no se puede hacer un remove de
            // la lista que se está recorriendo con el foreach dentro del mismo foreach

            PlayerInfo p = null; // Creamos un objeto auxiliar p
            //p = new PlayerInfo(); // p es igual a un objeto genérico
            foreach (PlayerInfo player in m_Players) // Recorremos la lista de jugadores
            {
                if (player == null) // Si el player es null (es decir, si se ha desconectado)
                {
                    m_PlayersNotOrdered.Remove(player); // Eliminamos el jugador de la lista sin ordenar
                    p = player; // p se iguala al player, es decir, a null porque se desconectó
                }
            }

            if (p == null) // si p es null significa que hay que eliminarlo de la lista
            {
                m_Players.Remove(p); // Eliminamos el jugador
                if(gameStartManager.gameStarted)
                    return; // Volvemos a empezar el bucle, porque hay que comprobar si hay más players nulos
            }


            // Update car arc-lengths

            m_arcLengths = new float[m_Players.Count];

            for (int i = 0; i < m_Players.Count; ++i)
            {
                // ComputeCarArcLength calcula la longitud de arco para el coche con id i
                // arcLengths[i] guarda la longitud de arco ordenados por id
                //m_arcLengths[m_Players[i].ID] = ComputeCarArcLength(m_Players[i].ID); // POR ESO TENEMOS QUE HACER COMPUTE POR POSICION, PILLANDO LA ID DEL QUE VA PRIMERO, SEGUNDO...
                m_arcLengths[i] = ComputeCarArcLength(i);
            }

            //if (gameStartManager.gameStarted)
            //{
            //    //playersArcLengths[i] = m_arcLengths[i];
            //    foreach (PlayerInfo player in m_Players)
            //    {
            //        if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
            //            player.GetComponent<SetupPlayer>().CmdUpdatePlayerArcLengthsList(m_arcLengths);
            //    }
            //}

            //if (gameStartManager.gameStarted)
            //{
            //    // Este método la lista de jugadores según las longitudes de arco por posición
            //    m_Players.Sort(new PlayerInfoComparer(playersArcLengths.ToArray<float>()));
            //}
            //else
            //{
            // Este método la lista de jugadores según las longitudes de arco por posición
            //m_PlayersNotOrdered = m_Players.ToList<PlayerInfo>();

            m_Players.Sort(new PlayerInfoComparer(m_arcLengths));

            // Se asigna la posición
            for (int i = 0; i < m_Players.Count; ++i)
            {
                m_Players[i].CurrentPosition = i;
            }
        }

        string myRaceOrder = "";
        foreach (var _player in m_Players)
        {
            myRaceOrder += _player.Name + "\n";
        }
        uiManager.UpdatePlayerNames(myRaceOrder);
    }

    float ComputeCarArcLength(int ID)
    {
        // Debug.LogWarning("INFO " + ID + ": " + m_Players[ID].ToString());
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

        if (gameStartManager.gameStarted)
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