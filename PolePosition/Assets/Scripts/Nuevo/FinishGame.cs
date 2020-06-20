using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Clase para gestionar el final de partida
/// </summary>
public class FinishGame : NetworkBehaviour
{
    #region Variables privadas
    private bool endedGame;             // Booleano que indica si la partida ha acabado
    private bool hasShownFinalGUI;      // Booleano que indica si ya ha mostrado la pantalla final
    private UIManager m_UIManager;      // Referencia al UIManager
    private PolePositionManager m_PPM;  // Referencia al PolePositionManager
    private LapManager m_lapManager;    // Referencia al LapManager
    #endregion

    #region Variables publicas
    [Tooltip("Textos de final de partida")]
    public Text[] endTexts;
    [Tooltip("Lista final de jugadores ordenados por ID")]
    public List<PlayerInfo> m_playersNotOrdered;
    [Tooltip("Lista final de jugadores ordenados por posicion")]
    public List<PlayerInfo> m_players;
    [Tooltip("Texto de cuenta atrás")]
    public Text endTimerText;
    #endregion

    #region Unity CallBacks
    /// <summary>
    /// Función Awake, que inicializa las siguientes variables
    /// </summary>
    void Awake()
    {
        // Se adquiere la referencia al UIManager
        m_UIManager = FindObjectOfType<UIManager>();

        // Se adquiere la referencia al Pole Position Manager
        m_PPM = FindObjectOfType<PolePositionManager>();
    }

    /// <summary>
    /// Función Start, que inicializa el lap manager
    /// </summary>
    private void Start()
    {
        // Se adquiere la referencia al Lap Manager
        m_lapManager = FindObjectOfType<LapManager>();
    }

    /// <summary>
    /// Función Update, que se ejecuta cada frame
    /// </summary>
    void Update()
    {
        // Si el juego no ha terminado
        if (!endedGame)
        {
            // Si el Pole Position Manager indicia que el juego ha terminado
            if (m_PPM.gameHasEnded)
            {
                // Establecemos que el juego ha terminado y hacemos una copia de las listas del Pole Position Manager
                endedGame = true;
                m_playersNotOrdered = m_PPM.m_PlayersNotOrdered.ToList<PlayerInfo>();
                m_players = m_PPM.m_Players.ToList<PlayerInfo>();
            }
        }
        // Si el juego ya ha terminado
        else
        {
            // Escribimos los textos de final de partida
            endTexts[0].text = "Player\n\n";
            endTexts[1].text = "Position\n\n";
            endTexts[2].text = "Total time\n\n";
            endTexts[3].text = "Best lap\n\n";

            // Creamos arrays auxiliares para acceder a la información del lap manager
            string[] totalTimers = { m_lapManager.player1TotalTimer, m_lapManager.player2TotalTimer, m_lapManager.player3TotalTimer, m_lapManager.player4TotalTimer };
            string[] bestTimers = { m_lapManager.player1BestTimer, m_lapManager.player2BestTimer, m_lapManager.player3BestTimer, m_lapManager.player4BestTimer };

            int[] finalPositions = { m_lapManager.endPos1, m_lapManager.endPos2, m_lapManager.endPos3, m_lapManager.endPos4 };

            // Por cada jugador, escribimos su información en la pantalla final
            for (int i = 0; i < m_playersNotOrdered.Count; i++)
            {
                if (finalPositions[i] == -1)
                {
                    finalPositions[i] = m_players[i].ID;
                }

                endTexts[0].text += m_playersNotOrdered[finalPositions[i]].Name + "\n";
                endTexts[1].text += (i + 1) + "\n";
                if (m_playersNotOrdered[finalPositions[i]].hasFinished)
                {
                    if (totalTimers[m_playersNotOrdered[finalPositions[i]].ID] == "")
                    {
                        endTexts[2].text += m_playersNotOrdered[finalPositions[i]].lapTotalMinutes + ":"
                            + m_playersNotOrdered[finalPositions[i]].lapTotalSeconds + ":"
                            + m_playersNotOrdered[finalPositions[i]].lapTotalMiliseconds + "\n";
                    }
                    else
                        endTexts[2].text += totalTimers[m_playersNotOrdered[finalPositions[i]].ID] + "\n";
                }
                else
                    endTexts[2].text += "NF\n";

                if (bestTimers[m_playersNotOrdered[finalPositions[i]].ID] != "")
                    endTexts[3].text += bestTimers[m_playersNotOrdered[finalPositions[i]].ID] + "\n";
                else
                    endTexts[3].text += "---\n";
            }

            // Se le impide el movimiento a cada coche
            foreach (PlayerInfo p in m_playersNotOrdered)
                p.canMove = false;

            // Escondemos las otras interfaces y mostramos la pantalla final
            if (!hasShownFinalGUI)
            {
                GetComponent<GameStartManager>().totalTimer.StopTimer();
                m_UIManager.inGameHUD.SetActive(false);
                m_UIManager.waitFinishHUD.SetActive(false);
                m_UIManager.gameFinishHUD.SetActive(true);
                hasShownFinalGUI = true;
            }

            // Añadimos al botón de volver el callback de la función de volver al menú
            m_UIManager.buttonFinishReturn.onClick.AddListener(() => ReturnToMenu());
        }
    }

    #endregion

    #region Other Functions

    /// <summary>
    /// Se apaga el servidor y se recarga la escena.
    /// </summary>
    private void ReturnToMenu()
    {
        // Si es el servidor
        if (isServer)
        {
            // Y está en modos Server Only, paramos el servidor
            if (isServerOnly)
            {
                NetworkManager.singleton.StopServer();
            }
            // Si es a la vez un cliente (es el host), paramos el host
            else
            {
                NetworkManager.singleton.StopHost();
            }
        }
        // Si es un cliente, paramos el cliente
        else
        {
            NetworkManager.singleton.StopClient();
        }

        // Se apaga el servidor
        NetworkServer.Shutdown();

        // Se reinicia la escena
        SceneManager.LoadScene(0);
    }

    #endregion

    #region Network Functions

    #region Rpc Functions

    /// <summary>
    /// Función que actualiza el texto que muestra el tiempo que falta para acabar
    /// </summary>
    /// <param name="timeTE">Tiempo que falta para acabar (time to end)</param>
    [ClientRpc]
    public void RpcUpdateEndTimerText(int timeTE)
    {
        if (timeTE >= 10)
            endTimerText.text = "00:" + timeTE;
        else if (timeTE >= 0)
            endTimerText.text = "00:0" + timeTE;
    }

    /// <summary>
    /// Función que actualiza la variable readyToShowFinalScreen del lap manager
    /// </summary>
    [ClientRpc]
    public void RpcUpdateReadyToShow()
    {
        if (!m_lapManager)
            m_lapManager = FindObjectOfType<LapManager>();
        m_lapManager.readyToShowFinalScreen = true;
    }

    #endregion

    #region Command Functions

    /// <summary>
    /// Función que se ejecuta en el servidor y actualiza el tiempo para acabar del lap manager
    /// </summary>
    /// <param name="tte"></param>
    [Command]
    public void CmdUpdateEndTime(float tte)
    {
        if (!m_lapManager)
            m_lapManager = FindObjectOfType<LapManager>();
        m_lapManager.timeToEnd = tte;
    }

    #endregion

    #endregion
}
