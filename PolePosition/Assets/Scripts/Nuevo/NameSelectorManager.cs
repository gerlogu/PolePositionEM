using UnityEngine.UI;

/// <summary>
/// Controlador que contiene los métodos del selector de nombres
/// </summary>
public class NameSelectorManager
{
    InputField inputFieldPlayerName; // Campo donde el jugador escribe su nombre
    string defaultName = "Player";   // Nombre por defecto del jugador

    /// <summary>
    /// Constructor de la clase.
    /// </summary>
    /// <param name="_inputFieldPlayerName">Referencia al inputField ubicado en el canvas</param>
    public NameSelectorManager(InputField _inputFieldPlayerName, string _defaultName)
    {
        inputFieldPlayerName = _inputFieldPlayerName;
        defaultName = _defaultName;
    }

    /// <summary>
    /// Comprueba el texto del inputField y actualizamos el nombre del jugador.
    /// </summary>
    /// <returns>Nombre nuevo del jugador</returns>
    public void CheckPlayerName(out string currentName)
    {
        // Si el inputField tiene texto, se actualiza el nombre del jugador
        currentName = (inputFieldPlayerName.text != "") ? inputFieldPlayerName.text : defaultName;
    }
}
