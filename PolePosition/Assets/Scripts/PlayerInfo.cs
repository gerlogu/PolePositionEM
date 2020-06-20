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

    // Tipo de coche
    public CarType carType { get; set; }

    // Enumerador de los tipos de coches posibles
    public enum CarType
    {
        GREEN = 0,
        ORANGE = 1,
        RED = 2,
        WHITE = 3
    }

    /// <summary>
    /// Se actualizas el tipo de coche en función de un índice introducido.
    /// </summary>
    /// <param name="index">Índice</param>
    public void SetCarType(int index)
    {
        switch (index)
        {
            case 0:
                carType = CarType.GREEN;
                break;
            case 1:
                carType = CarType.ORANGE;
                break;
            case 2:
                carType = CarType.RED;
                break;
            case 3:
                carType = CarType.WHITE;
                break;
            default:
                carType = CarType.GREEN;
                break;
        }
    }

    // Bool que determina si el jugador se puede mover
    public bool canMove { get; set; }

    // Tiempo total
    public string lapTotalMiliseconds { get; set; }
    public string lapTotalSeconds { get; set; }
    public string lapTotalMinutes { get; set; }

    // Tiempo mejor vuelta
    public int lapBestMiliseconds { get; set; }
    public int lapBestSeconds { get; set; }
    public int lapBestMinutes { get; set; }

    //Booleano que indica si ha terminado la carrera
    public bool hasFinished { get; set; }


    /// <summary>
    /// Imprime información del jugador.
    /// </summary>
    /// <returns>Nombre del jugador</returns>
    public override string ToString()
    {
        return "Name: " + Name + ", ID: " + ID + ", CurrentPosition: " + CurrentPosition + ", CurrentLap: " + CurrentLap + ", canMove: " + canMove + ", hasFinished: " + hasFinished;
    }
}