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

    [SyncVar] public float timeToEnd = 20.0f;

    [SyncVar] public int endPos1 = -1;
    [SyncVar] public int endPos2 = -1;
    [SyncVar] public int endPos3 = -1;
    [SyncVar] public int endPos4 = -1;
    [SyncVar] public int nextPos = 0;

    [SyncVar] public bool readyToShowFinalScreen = false;

    [Tooltip("Número total de vueltas")] [SyncVar] public int totalLaps; // Por poner algo de momento

    public void RestartAllSyncVars()
    {
        player1Laps = -1;
        player2Laps = -1;
        player3Laps = -1;
        player4Laps = -1;

        player1Finished = false;
        player2Finished = false;
        player3Finished = false;
        player4Finished = false;

    // Cada una guarda la string del timer (quería usar SyncList pero son un infierno)
        player1TotalTimer = "";
        player2TotalTimer = "";
        player3TotalTimer = "";
        player4TotalTimer = "";

        player1BestTimer = "";
        player2BestTimer = "";
        player3BestTimer = "";
        player4BestTimer = "";

        endPos1 = -1;
        endPos2 = -1;
        endPos3 = -1;
        endPos4 = -1;
        nextPos = 0;

        readyToShowFinalScreen = false;
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
