using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameStartManager : MonoBehaviour
{
    [HideInInspector] public bool gameStarted = false;
    // [SerializeField] Text timerText;
    [SerializeField] Animator timerAnim;
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
    }


    public void UpdateGameStarted(int numJugadores, List<PlayerInfo> players)
    {
        m_Players = players;
        if (numJugadores > 1)
        {
            gameStarted = true;
            timerAnim.Play("Play");
        }
    }
}
