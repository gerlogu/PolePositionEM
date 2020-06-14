﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LapController : NetworkBehaviour
{
    #region Variables privadas
    private int laps = 0;
    public PlayerInfo m_playerInfo;                 // Referencia al PlayerInfo
    private DirectionDetector m_directionDetector;  // Referencia al DirectionDetector
    private UIManager m_UIManager;                  // Referencia al UIManager
    private bool malaVuelta = false;                // Booleano que indica si la vuelta es mala (marcha atrás)
    public bool canLap = false;
    #endregion

    #region Variables publicas
    [Tooltip("Número total de vueltas")] public int totalLaps = 3; // Por poner algo de momento
    #endregion

    //void UpdateLaps(int anterior, int nuevo)
    //{
    //    m_playerInfo.CurrentLap = nuevo;
    //}

    /// <summary>
    /// Función Awake, que inicializa las siguientes variables.
    /// </summary>
    void Awake()
    {
        // Se obtiene el PlayerInfo
        m_playerInfo = this.GetComponent<PlayerInfo>();

        // Se obtiene el DirectionDetector
        m_directionDetector = this.GetComponent<DirectionDetector>();

        // Se obtiene el UIManager y se establece el texto de las vueltas
        m_UIManager = FindObjectOfType<UIManager>();
        m_UIManager.UpdateLaps(1, totalLaps);
    }

    private void Update()
    {
        //m_playerInfo.CurrentLap++;
        Debug.Log("Vueltas de " + m_playerInfo.Name + ": " + m_playerInfo.CurrentLap);
    }

    /// <summary>
    /// Comprueba si el coche entra en un trigger
    /// </summary>
    /// <param name="other">El trigger en el que entra el coche</param>
    private void OnTriggerEnter(Collider other)
    {
        // (Vale a ver esto es un problema a ver si tu sabes arreglarlo)
        // El host funciona perfecto, pero el cliente no porque tiene el lapController desactivado
        // La cosa es que el cliente me está modificando las vueltas del host y no debe

        // Si toca la meta:
        if (other.CompareTag("FinishLine") && canLap)
        {
            // Si va en buena dirección:
            if (m_directionDetector.buenaDireccion)
            {
                // Si NO ha entrado marcha atrás previamente:
                if (!malaVuelta)
                {
                    m_playerInfo.CurrentLap++;
                    m_UIManager.UpdateLaps(m_playerInfo.CurrentLap, totalLaps);
                    m_directionDetector.haCruzadoMeta = true;
                }
                // Si había entrado marcha atrás previamente:
                else
                {
                    malaVuelta = false;
                    m_playerInfo.CurrentLap++;
                }
            }
            // Si entra marcha atrás:
            else
            {
                malaVuelta = true;
                m_playerInfo.CurrentLap--;
            }
        }
    }
}