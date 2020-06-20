using Mirror;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    [Tooltip("Botón \"QUIT\"")]
    [SerializeField] private Button buttonQuit;

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
    [Tooltip("Menú de configuración")]
    [SerializeField] private GameObject configMenu;
    [Tooltip("Texto de la cantidad de jugadores de la carrera")]
    [SerializeField] private Text playersText;
    [Tooltip("Botón izquierdo")]
    [SerializeField] private Button buttonPlayersLeft;
    [Tooltip("Botón derecho")]
    [SerializeField] private Button buttonPlayersRight;
    [Tooltip("Texto de las vueltas de la carrera")]
    [SerializeField] private Text lapsText;
    [Tooltip("Botón izquierdo")]
    [SerializeField] private Button buttonLapsLeft;
    [Tooltip("Botón derecho")]
    [SerializeField] private Button buttonLapsRight;
    [Tooltip("Botón para aceptar la configuración")]
    [SerializeField] private Button buttonSelectConfig;
    [Tooltip("Número de jugadores actual")]
    [SerializeField] private int players = 2;
    [Tooltip("Número de vueltas actual")]
    [SerializeField] private int laps = 3;

    [Header("References")]
    [Tooltip("Referencia de un game start manager de la escena")]
    [SerializeField] private GameStartManager m_GameStartManager;
    [Tooltip("Referencia de un lap manager de la escena")]
    [SerializeField] private LapManager m_LapManager;
    
    [Header("Texto de error")]
    [Tooltip("Cartel de error")]
    [SerializeField] private GameObject connectionError;
    [Tooltip("Botón para regresar al menú de inicio")]
    [SerializeField] private Button buttonReturn;

    [Header("Car Selector")]
    [Tooltip("Tipos de coche disponibles")]
    public Car[] cars;
    [Tooltip("Selector de coches")]
    public GameObject carSelector;
    [Tooltip("Botón izquierdo")]
    [SerializeField] private Button buttonLeftCar;
    [Tooltip("Botón derecho")]
    [SerializeField] private Button buttonRightCar;
    [Tooltip("Botón para seleccinar un coche")]
    [SerializeField] private Button buttonSelectCar;
    [Tooltip("Texto del tipo de coche actual")]
    [SerializeField] private Text carText;
    [Tooltip("Bool que determina si se puede seleccionar coche o no")]
    public bool canSelect = true;

    NameSelectorManager selectorManager;                   // Clase que contiene las funciones necesarias para el selector de nombres
    [HideInInspector] public string playerName = "player"; // Nombre introducido en el InputField
    [HideInInspector] public int carType = 0;              // Tipo de coche actual

    [Header("Wrong Direction")]
    [Tooltip("Cartel de dirección incorrecta")]
    public GameObject incorrectDirection;

    [Header("Finish HUD")]
    [Tooltip("Cartel de espera al menú final")]
    public GameObject waitFinishHUD;
    [Tooltip("Menú final")]
    public GameObject gameFinishHUD;
    [Tooltip("Botón del menú final para volver al menú de inicio")]
    public Button buttonFinishReturn;


    [Header("Car Selection Animation")]
    [Tooltip("Animator del selector de coches")]
    [SerializeField] private Animator anim;
    private int currentCar = 0; // Coche actual

    /// <summary>
    /// Función Awake.
    /// </summary>
    private void Awake()
    {
        m_NetworkManager = FindObjectOfType<NetworkManager>(); // Se busca el network manager en la escena
    }

    /// <summary>
    /// Función Start.
    /// </summary>
    private void Start()
    {
        selectorManager = new NameSelectorManager(inputFieldName, "Player");
        carSelector.SetActive(false);
        connectionError.SetActive(false);

        // Se asocian los botones a las diferentes funciones
        buttonHost.onClick.AddListener(() => ShowGameConfig(0));       // Name Selector (Host)
        buttonClient.onClick.AddListener(() => ShowNameSelector(1));   // Name Selector (Cliente)
        buttonServer.onClick.AddListener(() => ShowGameConfig(1));     // Servidor
        buttonQuit.onClick.AddListener(() => { Application.Quit(); }); // Botón para salir del juego
        ActivateMainMenu();                                            // Muestra por pantalla el menú principal
    }

    /// <summary>
    /// Muestra el texto de error de conexión por pantalla al estar la partida llena.
    /// </summary>
    public void ShowConnectionErrorMessage()
    {
        connectionError.SetActive(true);
        buttonReturn.onClick.AddListener(() => { SceneManager.LoadScene(0); }); // Asociamos la función de retorno al menú principal
    }

    /// <summary>
    /// Reinicio del menú de inicio.
    /// </summary>
    public void RestartMenu()
    {
        connectionError.SetActive(false); // Se oculta el menú de error
        inGameHUD.SetActive(false);       // Se oculta la interfaz In Game
        carType = 0;                      // Se reinicia el tipo de coche actual
        currentCar = 0;                   // Se reinicia el coche elegido
        mainMenu.SetActive(true);         // Se muestra nuevamente el menú de inicio
        anim.enabled = true;              // Se activa el animator para el selector de coches
        anim.Play("Idle");                // Se activa la animación inicial
    }

    /// <summary>
    /// Se muestra el menú de configuración de partida
    /// </summary>
    /// <param name="type"> 0: Host | 1: Servidor </param>
    private void ShowGameConfig(int type)
    {
        mainMenu.SetActive(false);  // Se oculta el menú de inicio
        configMenu.SetActive(true); // Se muestra el menú de configuración

        // Se puede elegir desde 1 vuelta a 9 vueltas para dar
        buttonLapsLeft.onClick.AddListener(() => { laps = (laps > 1) ? (laps - 1) : laps; });
        buttonLapsRight.onClick.AddListener(() => { laps = (laps < 9) ? (laps + 1) : laps; });

        // Se puede elegir un partida para desde 2 hasta 4 jugadores
        buttonPlayersLeft.onClick.AddListener(() => { players = (players > 2) ? players - 1 : players;});
        buttonPlayersRight.onClick.AddListener(() => { players = (players < 4) ? players + 1 : players; });

        // Botón para confirmar la configuración actual
        buttonSelectConfig.onClick.AddListener(() => 
        {
            m_GameStartManager.minPlayers = players; // Se actualiza el número de jugadores necesarios para comenzar la partida
            m_LapManager.totalLaps = laps;           // Se actualiza el número de vueltas para terminar la partida
            configMenu.SetActive(false);             // Se desactiva el menú de configuración
            
            switch (type)
            {
                case 0:
                    ShowNameSelector(0); // Se muestra el selector de nombre
                    break;
                case 1:
                    StartServer(); // Se inicia el servidor con la configuración elegida
                    break;
                default:
                    Debug.LogError("<color=red>ERROR</color> -> Opción no permitida. Ubicación: ShowGameConfig().");
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

        // Si el menú de configuración se encuentra activado, se actualizan los textos que muestran el valor de las variables
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
        carSelector.SetActive(false);  // Se oculta el menú del selector de coches

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

    /// <summary>
    /// Se actualiza el coche elegido hacia la derecha si está disponible.
    /// </summary>
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
                Debug.LogError("<color=red>ERROR</color> -> Valor no permitido. Ubicación: UpdateCarRight()");
                break;
        }
    }

    /// <summary>
    /// Se actualiza el coche elegido hacia la izquierda si está disponible.
    /// </summary>
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
                Debug.LogError("<color=red>ERROR</color> -> Valor no permitido. Ubicación: UpdateCarLeft()");
                break;
        }
    }

    /// <summary>
    /// Seleccionador de un coche e inicializador de la partida
    /// </summary>
    /// <param name="carType">Tipo de coche</param>
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
        m_NetworkManager.networkAddress = (inputFieldIP.text != "") ? inputFieldIP.text : "localhost";
        m_NetworkManager.StartClient(); // Se inicia el cliente
        ActivateInGameHUD();            // Se activa el GUI de la partida
    }

    /// <summary>
    /// Inicializa un servidor para que puedan jugar clientes en él.
    /// </summary>
    private void StartServer()
    {
        m_NetworkManager.StartServer(); // Se inicia el servidor
        ActivateInGameHUD();            // Se activa el GUI de la partida
    }
}