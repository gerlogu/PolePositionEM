using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;


public class HostManager : MonoBehaviour
{
    public bool isHost = false;
    public Barrier playersReady = new Barrier(2);

    public void UpdatePlayers()
    {
        playersReady.SignalAndWait();
        Debug.Log("Barrera iniciada");
    }
}
