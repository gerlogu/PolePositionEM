using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manager de la interfaz de usuario (Menú principal, selector de nombres o GUI).
/// </summary>
public class UIManager : MonoBehaviour
{
    [Tooltip("Determina si se muestra o no la GUI cuando inicie la partida")]
    public bool showGUI = true;

    private NetworkManager m_NetworkManager; // Network Manager

    [Header("Main Menu")]
    [Tooltip("Objeto Main Menu del Canvas")]
    [SerializeField] private GameObject mainMenu;
    [Tooltip("Botón \"HOST (SERVER + CLIENT)\"")]
    [SerializeField] private Button buttonHost;
    [Tooltip("Botón \"CLIENT\"")]
    [SerializeField] private Button buttonClient;
    [Tooltip("Botón \"SERVER ONLY\"")]
    [SerializeField] private Button buttonServer;
    [Tooltip("Input Field para el servidor")]
    [SerializeField] private InputField inputFieldIP;

    [Header("In-Game HUD")]
    [Tooltip("HUD In Game")]
    public GameObject inGameHUD;
    [Tooltip("Velocidad del vehículo")]
    [SerializeField] private Text textSpeed;
    [Tooltip("Vueltas dadas durante la carrera")]
    [SerializeField] private Text textLaps;
    [Tooltip("Posición (o puesto) del vehículo")]
    [SerializeField] private Text textPosition;
    public Text textNumPlayers;

    [Header("Name Selector")]
    [Tooltip("Objeto Name Selector")]
    [SerializeField] private GameObject nameSelector;
    [Tooltip("Botón \"Select Name\"")]
    [SerializeField] private Button buttonPlayerName;
    [Tooltip("Input Field para el nombre del jugador")]
    [SerializeField] private InputField inputFieldName;

    [Header("Game Configuration Menu")]
    [SerializeField] private GameObject configMenu;
    [SerializeField] private Text playersText;
    [SerializeField] private Button buttonPlayersLeft;
    [SerializeField] private Button buttonPlayersRight;
    [SerializeField] private Text lapsText;
    [SerializeField] private Button buttonLapsLeft;
    [SerializeField] private Button buttonLapsRight;
    [SerializeField] private Button buttonSelectConfig;
    [SerializeField] private GameStartManager m_GameStartManager;
    [SerializeField] private LapManager m_LapManager;

    public int players = 2;
    public int laps = 3;
    

    [Header("Car Selector")]
    public Car[] cars;
    public GameObject carSelector;
    [SerializeField] private Button buttonLeftCar;
    [SerializeField] private Button buttonRightCar;
    [SerializeField] private Button buttonSelectCar;
    [SerializeField] private Text carText;
    public bool canSelect = true;

    NameSelectorManager selectorManager;                   // Clase que contiene las funciones necesarias para el selector de nombres
    [HideInInspector] public string playerName = "player"; // Nombre introducido en el InputField
    [HideInInspector] public int carType = 0;

    [Header("Wrong Direction")]
    public GameObject incorrectDirection;

    [Header("Finish HUD")]
    public GameObject waitFinishHUD;
    public GameObject gameFinishHUD;


    [Header("Car Selection Animation")]
    [SerializeField] private Animator anim;
    [SerializeField] private int currentCar = 0;

    private void Awake()
    {
        m_NetworkManager = FindObjectOfType<NetworkManager>(); // Se busca el network manager en la escena
    }

    private void Start()
    {
        selectorManager = new NameSelectorManager(inputFieldName, "Player");
        carSelector.SetActive(false);

        // Se asocian los botones a las diferentes funciones
        buttonHost.onClick.AddListener(() => ShowGameConfig(0));     // Name Selector (Host)
        buttonClient.onClick.AddListener(() => ShowNameSelector(1)); // Name Selector (Cliente)
        buttonServer.onClick.AddListener(() => ShowGameConfig(1));   // Servidor
        ActivateMainMenu();                                          // Muestra por pantalla el menú principal
    }

    private void ShowGameConfig(int type)
    {
        mainMenu.SetActive(false);
        configMenu.SetActive(true);
        buttonLapsLeft.onClick.AddListener(() => { laps = (laps > 1) ? (laps - 1) : laps; });
        buttonLapsRight.onClick.AddListener(() => { laps = (laps < 9) ? (laps + 1) : laps; });

        buttonPlayersLeft.onClick.AddListener(() => { players = (players > 1) ? players - 1 : players;});
        buttonPlayersRight.onClick.AddListener(() => { players = (players < 4) ? players + 1 : players; });

        buttonSelectConfig.onClick.AddListener(() => 
        {
            m_GameStartManager.minPlayers = players;
            m_LapManager.totalLaps = laps;
            configMenu.SetActive(false);
            
            switch (type)
            {
                case 0:
                    ShowNameSelector(0);
                    break;
                case 1:
                    StartServer();
                    break;
                default:
                    break;
            }
        });
    }

    private void Update()
    {
        // Para no actualizar esta variable constantemente y evitar cálculos innecesarios
        // si no el nameSelector no se encuentra activo, no se actualiza más el nombre del jugador.
        // *
        // Dicho de otra forma, el nombre del jugador se encuentra actualizándose durante la pantalla
        // de selección de nombre.
        if (nameSelector.activeSelf)
        {
            selectorManager.CheckPlayerName(out playerName); // Se actualiza el nombre del jugador
        }

        if (configMenu.activeSelf)
        {
            playersText.text = players.ToString();
            lapsText.text    = laps.ToString();
        }
    }

    /// <summary>
    /// Actualiza el texto en pantalla donde aparece la velocidad del coche.
    /// </summary>
    /// <param name="speed">Velocidad del vehículo</param>
    public void UpdateSpeed(int speed)
    {
        textSpeed.text = "Speed " + speed + " Km/h"; // Se actualiza el texto con la velocidad nueva
    }

    /// <summary>
    /// Actualiza las posiciones de los jugadores
    /// </summary>
    public void UpdatePlayerNames(string playerName)
    {
        textPosition.text = playerName; // Se actualiza el texto con el nombre nuevo
    }

    /// <summary>
    /// Actualiza las vueltas de los jugadores
    /// </summary>
    public void UpdateLaps(int currentLap, int totalLaps)
    {
        textLaps.text = "LAP: " + currentLap + "/" + totalLaps; // Se actualiza el texto con la nueva vuelta
    }

    /// <summary>
    /// Muestra por pantalla el menú principal.
    /// </summary>
    private void ActivateMainMenu()
    {
        mainMenu.SetActive(true);   // Se muestra el menú de inicio
        inGameHUD.SetActive(false); // Se oculta la interfaz In Game
    }

    /// <summary>
    /// Muestra por pantalla el menú In Game (donde se muestran las velocidades, etc.).
    /// </summary>
    private void ActivateInGameHUD()
    {
        mainMenu.SetActive(false);     // Se oculta el menú principal
        nameSelector.SetActive(false); // Se oculta el selector de nombres
        carSelector.SetActive(false);

        // Si showGUI está a true (se desea mostrar la interfaz)
        if (showGUI)
        {
            inGameHUD.SetActive(true); // Se muestra la interfaz In Game
        }
    }

    /// <summary>
    /// Muestra el selector de nombres.
    /// </summary>
    /// <param name="type"> 0: Host | 1: Cliente </param>
    private void ShowNameSelector(int type)
    {
        mainMenu.SetActive(false);    // Se oculta el menú principal
        nameSelector.SetActive(true); // Se muestra el selector de nombres

        buttonPlayerName.onClick.AddListener(() => ShowCarSelector(type));  // Host
    }

    /// <summary>
    /// Muestra el selector de coches.
    /// </summary>
    /// <param name="type"> 0: Host | 1: Cliente </param>
    private void ShowCarSelector(int type)
    {
        nameSelector.SetActive(false); // Se muestra el selector de nombres
        canSelect = false;
        anim.SetTrigger("ShowCarSelector");
        buttonSelectCar.onClick.AddListener(() => SelectCar(currentCar, type));
        buttonLeftCar.onClick.AddListener(() => UpdateCarLeft());
        buttonRightCar.onClick.AddListener(() => UpdateCarRight());
    }

    private void UpdateCarRight()
    {
        if (!canSelect || currentCar == 3)
            return;

        canSelect = false;
        int carBefore = currentCar;
        currentCar++;
        switch (carBefore)
        {
            case 0:
                anim.SetTrigger("ShowOrangeCar");
                carText.text = "ORANGE";
                break;
            case 1:
                anim.SetTrigger("ShowRedCar");
                carText.text = "RED";
                break;
            case 2:
                anim.SetTrigger("ShowWhiteCar");
                carText.text = "WHITE";
                break;
            default:
                break;
        }
    }

    private void UpdateCarLeft()
    {
        if (!canSelect || currentCar == 0)
            return;

        canSelect = false;
        int carBefore = currentCar;
        currentCar--;
        switch (carBefore)
        {
            case 0:
                break;
            case 1:
                anim.SetTrigger("ShowGreenCar");
                carText.text = "GREEN";
                break;
            case 2:
                anim.SetTrigger("ShowOrangeCar");
                carText.text = "ORANGE";
                break;
            case 3:
                anim.SetTrigger("ShowRedCar");
                carText.text = "RED";
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="carType"></param>
    /// <param name="type"> 0: Host | 1: Cliente </param>
    private void SelectCar(int carType, int type)
    {
        this.carType = carType;
        anim.SetTrigger("StartRace");
        anim.enabled = false;
        switch (type)
        {
            case 0:
                StartHost();
                break;
            case 1:
                StartClient();
                break;
        }
    }

    /// <summary>
    /// Se conecta el Host a su propio servidor.
    /// </summary>
    private void StartHost()
    {
        m_NetworkManager.StartHost(); // Se inicia el Host
        ActivateInGameHUD();          // Se activa el GUI de la partida
    }

    /// <summary>
    /// Introduce a un cliente en el servidor administrado por un host,
    /// cuya IP es la escrita en el Input Field.
    /// </summary>
    private void StartClient()
    {

        m_NetworkManager.StartClient(); // Se inicia el cliente
        m_NetworkManager.networkAddress = inputFieldIP.text; // Se ajusta la IP
        ActivateInGameHUD(); // Se activa el GUI de la partida
    }

    /// <summary>
    /// NI IDEA
    /// </summary>
    private void StartServer()
    {
        m_NetworkManager.StartServer(); // Se inicia el servidor
        ActivateInGameHUD();            // Se activa el GUI de la partida
    }
}