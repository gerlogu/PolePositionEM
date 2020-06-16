using Mono.CecilX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinishGame : MonoBehaviour
{
    #region Variables privadas
    private bool endedGame;
    private bool hasShownFinalGUI;
    private UIManager m_UIManager;
    private PolePositionManager m_PPM;
    #endregion

    #region Variables publicas
    public Text[] endTexts;
    public List<PlayerInfo> m_players;
    public Text endTimerText;
    #endregion

    void Awake()
    {
        m_UIManager = FindObjectOfType<UIManager>();
        m_PPM = FindObjectOfType<PolePositionManager>();
    }

    void Update()
    {
        if (!endedGame)
        {
            if (m_PPM.gameHasEnded)
            {
                endedGame = true;
                m_players = m_PPM.m_Players;
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

                foreach (PlayerInfo p in m_players)
                {
                    endTexts[0].text += p.Name + "\n";
                    endTexts[1].text += (p.CurrentPosition + 1) + "\n";
                    if (p.hasFinished)
                        endTexts[2].text += p.lapTotalMinutes + ":" + p.lapTotalSeconds + ":" + p.lapTotalMiliseconds + "\n";
                    else
                        endTexts[2].text += "NF\n";
                    endTexts[3].text += p.lapBestMinutes + ":" + p.lapBestSeconds + ":" + p.lapBestMiliseconds + "\n";
                }

                m_UIManager.waitFinishHUD.SetActive(false);
                m_UIManager.gameFinishHUD.SetActive(true);
            }
        }
    }

    public void updateEndTimerText(int timeTE)
    {
        if (timeTE >= 10)
            endTimerText.text = "00:" + timeTE;
        else if (timeTE >= 0)
            endTimerText.text = "00:0" + timeTE;

        //Debug.LogWarning("ACTUALIZO: " + timeTE);
    }
}
