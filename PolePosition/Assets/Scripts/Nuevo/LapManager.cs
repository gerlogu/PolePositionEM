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

    private void Awake()
    {
        //laps = new SyncListInt();
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
