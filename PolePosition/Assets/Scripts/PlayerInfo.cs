using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// Clase contenedora de la información del jugador.
/// </summary>
public class PlayerInfo : MonoBehaviour
{
    // Nombre del jugador
    public string Name { get; set; }

    // ID del jugador
    public int ID { get; set; }

    // Posición actual del jugador
    public int CurrentPosition { get; set; }

    // Vuelta en la que se encuentra el jugador actualmente
    public int CurrentLap { get; set; }

    /// <summary>
    /// Imprime información del jugador.
    /// </summary>
    /// <returns>Nombre del jugador</returns>
    public override string ToString()
    {
        return Name;
    }
}