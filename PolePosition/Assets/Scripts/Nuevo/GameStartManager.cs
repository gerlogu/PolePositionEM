using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;
public class GameStartManager : MonoBehaviour
{
    [HideInInspector] public bool gameStarted = false;
    // [SerializeField] Text timerText;
    [SerializeField] Animator timerAnim;
    [SerializeField] GameObject semaphore;
    [SerializeField] Material[] stateGreen;
    [SerializeField] Material[] stateOrange;
    [SerializeField] Material[] stateRed;
    List<PlayerInfo> m_Players;
    float timer = 3;

    public void Update()
    {
        if (!gameStarted)
            return;

        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            foreach (PlayerInfo g in m_Players)
            {
                g.canMove = gameStarted;
            }
            
        }

        if(timer < 2.55f && timer > 1.65f)
        {
            semaphore.GetComponent<MeshRenderer>().materials = stateRed;
        }else if(timer <= 1.65f && timer > 0.5f)
        {
            semaphore.GetComponent<MeshRenderer>().materials = stateOrange;
        }else if(timer <= 0.5f)
        {
            semaphore.GetComponent<MeshRenderer>().materials = stateGreen;
        }
    }


    public void UpdateGameStarted(int numJugadores, List<PlayerInfo> players)
    {
        m_Players = players.ToList<PlayerInfo>();
        if (numJugadores > 1)
        {
            gameStarted = true;
            timerAnim.Play("Play");
        }
    }
}
