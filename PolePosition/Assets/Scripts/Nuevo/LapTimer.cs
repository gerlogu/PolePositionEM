using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Crónometro con las variables formateadas, tanto como strings como enteros.
/// </summary>
public class LapTimer
{
    #region Variables Públicas
    // Datos como cadenas de caracteres
    [HideInInspector] public string hours;
    [HideInInspector] public string minutes;
    [HideInInspector] public string seconds;
    [HideInInspector] public string miliseconds;
    // Datos como enteros
    [HideInInspector] public int iHours;
    [HideInInspector] public int iMinutes;
    [HideInInspector] public int iSeconds;
    [HideInInspector] public int iMiliseconds;
    #endregion

    #region Variables Privadas
    private Stopwatch timer; // Timer de la vuelta
    #endregion

    /// <summary>
    /// Constructor vacío de la clase.
    /// </summary>
    public LapTimer()
    {
        timer = new Stopwatch();
    }

    /// <summary>
    /// Inicia el timer.
    /// </summary>
    public void StartTimer()
    {
        timer.Start();
    }

    /// <summary>
    /// Detiene el timer.
    /// </summary>
    public void StopTimer()
    {
        timer.Stop();
    }

    /// <summary>
    /// Reinicia el timer desde cero.
    /// </summary>
    public void RestartTimer()
    {
        if (!timer.IsRunning)
        {
            timer.Start();
        }
        timer.Restart();
    }

    /// <summary>
    /// Se calculan los valores de las horas, minutos, segundos y milisegundos; tanto como strings (formateado) como enteros.
    /// </summary>
    public void CalculateTime()
    {
        int _hours = Mathf.RoundToInt((float)timer.Elapsed.Hours);
        int _minutes = Mathf.RoundToInt((float)timer.Elapsed.Minutes);
        int _seconds = Mathf.RoundToInt((float)timer.Elapsed.Seconds);
        int _miliseconds = Mathf.RoundToInt((float)timer.Elapsed.Milliseconds);

        #region Horas
        if (_hours < 10)
        {
            hours = "0" + _hours;
        }
        else
        {
            hours = _hours.ToString();
        }
        #endregion

        #region Minutos
        if (_minutes < 10)
        {
            minutes = "0" + _minutes;
        }
        else
        {
            minutes = _minutes.ToString();
        }
        #endregion

        #region Segundos
        if (_seconds < 10)
        {
            seconds = "0" + _seconds;
        }
        else
        {
            seconds = _seconds.ToString();
        }
        #endregion

        #region Milisegundos
        if (_miliseconds < 10)
        {
            miliseconds = "00" + _miliseconds;
        }
        else if (_miliseconds < 100)
        {
            miliseconds = "0" + _miliseconds;
        }
        else
        {
            miliseconds = _miliseconds.ToString();
        }
        #endregion

        iHours = _hours;
        iMinutes = _minutes;
        iSeconds = _seconds;
        iMiliseconds = _miliseconds;
    }
}
