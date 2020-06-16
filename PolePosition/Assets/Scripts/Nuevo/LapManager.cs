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

    [SyncVar (hook = nameof(updateP1Laps))] public int player1Laps = -1;
    [SyncVar (hook = nameof(updateP1Laps))] public int player2Laps = -1;
    [SyncVar (hook = nameof(updateP1Laps))] public int player3Laps = -1;
    [SyncVar (hook = nameof(updateP1Laps))] public int player4Laps = -1;

    public List<PlayerInfo> m_players;
    
    public void updateP1Laps(int ant, int nuevo)
    {

    }

    public void updateP2Laps(int ant, int nuevo)
    {

    }

    public void updateP3Laps(int ant, int nuevo)
    {

    }

    public void updateP4Laps(int ant, int nuevo)
    {

    }

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
