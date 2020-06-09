using UnityEngine.UI;

/// <summary>
/// Controlador que contiene los métodos del selector de nombres
/// </summary>
public class NameSelectorManager
{
    InputField inputFieldPlayerName; // Campo donde el jugador escribe su nombre
    string currentName = "Player";   // Nombre del jugador

    /// <summary>
    /// Constructor de la clase.
    /// </summary>
    /// <param name="_inputFieldPlayerName">Referencia al inputField ubicado en el canvas</param>
    public NameSelectorManager(InputField _inputFieldPlayerName)
    {
        inputFieldPlayerName = _inputFieldPlayerName;
    }

    /// <summary>
    /// Comprueba el texto del inputField y actualizamos el nombre del jugador.
    /// </summary>
    /// <returns>Nombre nuevo del jugador</returns>
    public string CheckPlayerName()
    {
        // Si el inputField está seleccionado por el usuario y tiene texto, se actualiza el nombre del jugador
        currentName = (inputFieldPlayerName.isFocused && inputFieldPlayerName.text != "") ? inputFieldPlayerName.text : currentName;
        return currentName; // Se devuelve el nombre actual del jugador
    }
}
