using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Crónometro para las vueltas
/// </summary>
public class LapTimer
{
    #region Variables Públicas
    public string hours;
    public string minutes;
    public string seconds;
    public string miliseconds;
    #endregion

    #region Variables Privadas
    public int iHours;
    public int iMinutes;
    public int iSeconds;
    public int iMiliseconds;
    private Stopwatch timer;      // Timer de la vuelta
    #endregion

    public LapTimer()
    {
        timer = new Stopwatch();
    }

    public void StartTimer()
    {
        timer.Start();
    }

    public void StopTimer()
    {
        timer.Stop();
    }

    public void RestartTimer()
    {
        if (!timer.IsRunning)
        {
            timer.Start();
        }
        timer.Restart();
    }

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
        iSeconds = _miliseconds;
    }
}
