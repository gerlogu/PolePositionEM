using Mirror;
using UnityEngine;

/// <summary>
/// Controlador de las variables sincronizadas para las vueltas.
/// </summary>
public class LapManager : NetworkBehaviour
{
    [Header("Vueltas de los jugadores")]
    [Tooltip("Vueltas del jugador 1")]
    [SyncVar] public int player1Laps = -1;
    [Tooltip("Vueltas del jugador 2")]
    [SyncVar] public int player2Laps = -1;
    [Tooltip("Vueltas del jugador 3")]
    [SyncVar] public int player3Laps = -1;
    [Tooltip("Vueltas del jugador 4")]
    [SyncVar] public int player4Laps = -1;

    [Header("Jugadores que han terminado la carrera")]
    [Tooltip("¿El jugador 1 ha terminado la carrera?")]
    [SyncVar] public bool player1Finished = false;
    [Tooltip("¿El jugador 2 ha terminado la carrera?")]
    [SyncVar] public bool player2Finished = false;
    [Tooltip("¿El jugador 3 ha terminado la carrera?")]
    [SyncVar] public bool player3Finished = false;
    [Tooltip("¿El jugador 4 ha terminado la carrera?")]
    [SyncVar] public bool player4Finished = false;

    [Header("Tiempo total")]
    [Tooltip("Tiempo que ha tardado el jugador 1 en terminar la carrera")]
    [SyncVar] public string player1TotalTimer = "";
    [Tooltip("Tiempo que ha tardado el jugador 2 en terminar la carrera")]
    [SyncVar] public string player2TotalTimer = "";
    [Tooltip("Tiempo que ha tardado el jugador 3 en terminar la carrera")]
    [SyncVar] public string player3TotalTimer = "";
    [Tooltip("Tiempo que ha tardado el jugador 4 en terminar la carrera")]
    [SyncVar] public string player4TotalTimer = "";

    [Header("Tiempo de la mejor vuelta")]
    [Tooltip("Tiempo que ha tardado el jugador 1 en terminar su mejor vuelta")]
    [SyncVar] public string player1BestTimer = "";
    [Tooltip("Tiempo que ha tardado el jugador 2 en terminar su mejor vuelta")]
    [SyncVar] public string player2BestTimer = "";
    [Tooltip("Tiempo que ha tardado el jugador 3 en terminar su mejor vuelta")]
    [SyncVar] public string player3BestTimer = "";
    [Tooltip("Tiempo que ha tardado el jugador 4 en terminar su mejor vuelta")]
    [SyncVar] public string player4BestTimer = "";

    [Header("Final de Partida")]
    [Tooltip("Tiempo desde que llega el primer jugador a la meta para que termine la carrera")]
    [SyncVar] public float timeToEnd = 20.0f;
    [Tooltip("Bool que determina si se puede enseñar la pantalla final")]
    [SyncVar] public bool readyToShowFinalScreen = false;

    [Header("IDs de los jugadores que han terminado")]
    [Tooltip("ID del jugadror que ha terminado en primera posición")]
    [SyncVar] public int endPos1 = -1;
    [Tooltip("ID del jugador que ha terminado en segunda posición")]
    [SyncVar] public int endPos2 = -1;
    [Tooltip("ID del jugador que ha terminado en tercera posición")]
    [SyncVar] public int endPos3 = -1;
    [Tooltip("ID del jugador que ha terminado en cuarto posición")]
    [SyncVar] public int endPos4 = -1;
    [SyncVar] public int nextPos = 0;

    [Tooltip("Número total de vueltas")]
    [SyncVar] public int totalLaps;

    /// <summary>
    /// Función que reinicia todas las SyncVars a su valor inicial.
    /// </summary>
    public void RestartAllSyncVars()
    {
        player1Laps = -1;
        player2Laps = -1;
        player3Laps = -1;
        player4Laps = -1;

        player1Finished = false;
        player2Finished = false;
        player3Finished = false;
        player4Finished = false;

        player1TotalTimer = "";
        player2TotalTimer = "";
        player3TotalTimer = "";
        player4TotalTimer = "";

        player1BestTimer = "";
        player2BestTimer = "";
        player3BestTimer = "";
        player4BestTimer = "";

        endPos1 = -1;
        endPos2 = -1;
        endPos3 = -1;
        endPos4 = -1;
        nextPos = 0;

        readyToShowFinalScreen = false;
    }
}
