﻿using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinishGame : NetworkBehaviour
{
    #region Variables privadas
    private bool endedGame;
    private bool hasShownFinalGUI;
    private UIManager m_UIManager;
    private PolePositionManager m_PPM;
    private LapManager m_lapManager;
    #endregion

    #region Variables publicas
    public Text[] endTexts;
    public List<PlayerInfo> m_playersNotOrdered;
    public List<PlayerInfo> m_players;
    public Text endTimerText;
    #endregion

    void Awake()
    {
        m_UIManager = FindObjectOfType<UIManager>();
        m_PPM = FindObjectOfType<PolePositionManager>();
    }

    private void Start()
    {
        m_lapManager = FindObjectOfType<LapManager>();
    }

    void Update()
    {
        if (!endedGame)
        {
            if (m_PPM.gameHasEnded)
            {
                endedGame = true;
                m_playersNotOrdered = m_PPM.m_PlayersNotOrdered.ToList<PlayerInfo>();
                m_players = m_PPM.m_Players.ToList<PlayerInfo>();
            }
        }
        else
        {
            if (!hasShownFinalGUI)
            {
                hasShownFinalGUI = true;

                endTexts[0].text = "Player\n\n";
                endTexts[1].text = "Position\n\n";
                endTexts[2].text = "Total time\n\n";
                endTexts[3].text = "Best lap\n\n";

                string[] totalTimers = { m_lapManager.player1TotalTimer, m_lapManager.player2TotalTimer, m_lapManager.player3TotalTimer, m_lapManager.player4TotalTimer };
                string[] bestTimers = { m_lapManager.player1BestTimer, m_lapManager.player2BestTimer, m_lapManager.player3BestTimer, m_lapManager.player4BestTimer };

                int[] finalPositions = { m_lapManager.endPos1, m_lapManager.endPos2, m_lapManager.endPos3, m_lapManager.endPos4 };

                for (int i = 0; i < m_playersNotOrdered.Count; i++)
                {
                    if (finalPositions[i] == -1)
                    {
                        finalPositions[i] = m_players[i].ID;
                    }

                    Debug.LogWarning("FinalPositions " + i + ": " + finalPositions[i]);

                    endTexts[0].text += m_playersNotOrdered[finalPositions[i]].Name + "\n";
                    endTexts[1].text += (i + 1) + "\n";
                    if (m_playersNotOrdered[finalPositions[i]].hasFinished)
                        endTexts[2].text += totalTimers[m_playersNotOrdered[finalPositions[i]].ID] + "\n";
                    else
                        endTexts[2].text += "NF\n";

                    if (bestTimers[m_playersNotOrdered[finalPositions[i]].ID] != "")
                        endTexts[3].text += bestTimers[m_playersNotOrdered[finalPositions[i]].ID] + "\n";
                    else
                        endTexts[3].text += "---\n";
                }

                foreach (PlayerInfo p in m_playersNotOrdered)
                    p.canMove = false;

                m_UIManager.inGameHUD.SetActive(false);
                m_UIManager.waitFinishHUD.SetActive(false);

                m_UIManager.buttonFinishReturn.onClick.AddListener(() => ReturnToMenu());
                m_UIManager.gameFinishHUD.SetActive(true);

            }
        }
    }

    /// <summary>
    /// Se apaga el servidor y se recarga la escena.
    /// </summary>
    void ReturnToMenu()
    {
        if (isServer)
        {
            if (isServerOnly)
            {
                Debug.Log("StopServer ejecutado.");
                NetworkManager.singleton.StopServer();
            }
            else
            {
                Debug.Log("StopHost ejecutado.");

                //GetComponent<PolePositionManager>().StopServer();
                NetworkManager.singleton.StopHost();
            }
        }
        else
        {
            Debug.Log("StopClient ejecutado.");
            NetworkManager.singleton.StopClient();
        }


        Debug.Log("Apagando servidor...");
        NetworkServer.Shutdown();

        SceneManager.LoadScene(0);

        //m_lapManager.RestartAllSyncVars();
        //m_UIManager.gameFinishHUD.SetActive(false);
        //m_UIManager.RestartMenu();
    }


    [ClientRpc]
    public void RpcUpdateEndTimerText(int timeTE)
    {
        if (timeTE >= 10)
            endTimerText.text = "00:" + timeTE;
        else if (timeTE >= 0)
            endTimerText.text = "00:0" + timeTE;

        //Debug.LogWarning("ACTUALIZO: " + timeTE);
    }

    [ClientRpc]
    public void RpcUpdateReadyToShow()
    {
        if (!m_lapManager)
            m_lapManager = FindObjectOfType<LapManager>();
        m_lapManager.readyToShowFinalScreen = true;
    }

    [Command]
    public void CmdUpdateEndTime(float tte)
    {
        if (!m_lapManager)
            m_lapManager = FindObjectOfType<LapManager>();
        m_lapManager.timeToEnd = tte;
    }
}
