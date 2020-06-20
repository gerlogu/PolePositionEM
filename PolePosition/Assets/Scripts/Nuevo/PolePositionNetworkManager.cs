using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PolePositionNetworkManager : NetworkManager
{
    [SerializeField] PolePositionManager m_PolePositionManager;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //base.OnServerAddPlayer(conn);
    }
}
