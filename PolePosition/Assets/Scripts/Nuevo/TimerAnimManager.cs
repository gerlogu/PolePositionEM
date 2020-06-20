using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador de animación del contador inicial de la partida.
/// </summary>
public class TimerAnimManager : MonoBehaviour
{
    [Tooltip("Texto mostrado")]
    [SerializeField] Text timerText;

    /// <summary>
    /// Cambia el contenido del texto.
    /// </summary>
    /// <param name="text">Nuevo texto</param>
    public void SetText(string text)
    {
        timerText.text = text;
    }
}
