using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirror;
using UnityEngine;

public class LapManager : NetworkBehaviour
{
    //public SyncListInt laps;

    [SyncVar] public int player1Laps = -1;
    [SyncVar] public int player2Laps = -1;
    [SyncVar] public int player3Laps = -1;
    [SyncVar] public int player4Laps = -1;

    [SyncVar] public bool player1Finished = false;
    [SyncVar] public bool player2Finished = false;
    [SyncVar] public bool player3Finished = false;
    [SyncVar] public bool player4Finished = false;

    // Cada una guarda la string del timer (quería usar SyncList pero son un infierno)
    [SyncVar] public string player1TotalTimer = "";
    [SyncVar] public string player2TotalTimer = "";
    [SyncVar] public string player3TotalTimer = "";
    [SyncVar] public string player4TotalTimer = "";

    [SyncVar] public string player1BestTimer = "";
    [SyncVar] public string player2BestTimer = "";
    [SyncVar] public string player3BestTimer = "";
    [SyncVar] public string player4BestTimer = "";


    public List<PlayerInfo> m_players;

    [SyncVar] public int endPos1;
    [SyncVar] public int endPos2;
    [SyncVar] public int endPos3;
    [SyncVar] public int endPos4;
    [SyncVar] public int nextPos = 0;

    [SyncVar] public bool readyToShowFinalScreen = false;

    [Tooltip("Número total de vueltas")] [SyncVar] public int totalLaps; // Por poner algo de momento


    private void Awake()
    {
        //laps = new SyncListInt();
        m_players = GetComponent<PolePositionManager>().m_Players;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
