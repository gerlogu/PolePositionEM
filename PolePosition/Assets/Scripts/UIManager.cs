﻿using System;
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
    [SerializeField] private GameObject inGameHUD;
    [Tooltip("Velocidad del vehículo")]
    [SerializeField] private Text textSpeed;
    [Tooltip("Vueltas dadas durante la carrera")]
    [SerializeField] private Text textLaps;
    [Tooltip("Posición (o puesto) del vehículo")]
    [SerializeField] private Text textPosition;

    [Header("Name Selector")]
    [Tooltip("Objeto Name Selector")]
    [SerializeField] private GameObject nameSelector;
    [Tooltip("Botón \"Select Name\"")]
    [SerializeField] private Button buttonPlayerName;
    [Tooltip("Input Field para el nombre del jugador")]
    [SerializeField] private InputField inputFieldName;

    [Header("Car Selector")]
    [SerializeField] private Car[] cars;
    [SerializeField] private GameObject carSelector;
    [SerializeField] private Button buttonRedCar;
    [SerializeField] private Button buttonWhiteCar;
    [SerializeField] private Button buttonOrangeCar;
    [SerializeField] private Button buttonGreenCar;

    NameSelectorManager selectorManager;                       // Clase que contiene las funciones necesarias para el selector de nombres
    [HideInInspector] public string playerName = "player"; // Nombre introducido en el InputField

    private void Awake()
    {
        m_NetworkManager = FindObjectOfType<NetworkManager>(); // Se busca el network manager en la escena
    }

    private void Start()
    {
        selectorManager = new NameSelectorManager(inputFieldName);

        // Se asocian los botones a las diferentes funciones
        buttonHost.onClick.AddListener(() => ShowNameSelector(0));   // Name Selector (Host)
        buttonClient.onClick.AddListener(() => ShowNameSelector(1)); // Name Selector (Cliente)
        buttonServer.onClick.AddListener(() => StartServer());       // Servidor
        ActivateMainMenu();                                          // Muestra por pantalla el menú principal
    }

    private void Update()
    {
        playerName = selectorManager.CheckPlayerName(); // Se actualiza el nombre del personaje
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
        textPosition.text = playerName; // Se actualiza el texto con la velocidad nueva
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
        carSelector.SetActive(true);
        buttonGreenCar.onClick.AddListener(() => SelectCar(0, type));
        buttonOrangeCar.onClick.AddListener(() => SelectCar(1, type));
        buttonRedCar.onClick.AddListener(() => SelectCar(2, type));
        buttonWhiteCar.onClick.AddListener(() => SelectCar(3, type));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="carType"></param>
    /// <param name="type"> 0: Host | 1: Cliente </param>
    private void SelectCar(int carType, int type)
    {
        m_NetworkManager.playerCarMaterials = cars[carType].carMaterials;
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