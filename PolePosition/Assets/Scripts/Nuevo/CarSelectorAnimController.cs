using UnityEngine;

/// <summary>
/// Controlador de la animación de la cámara durante la selección de coche.
/// </summary>
public class CarSelectorAnimController : MonoBehaviour
{
    [Tooltip("Referencia del UIManager de la escena")]
    [SerializeField] private UIManager m_UIManager;

    /// <summary>
    /// Permite al jugador poder elegir coche.
    /// </summary>
    public void SetCarSelectorAvailable()
    {
        m_UIManager.canSelect = true;
    }

    /// <summary>
    /// Permite al jugador elegir coche y mostrar el menú de selección.
    /// </summary>
    public void ShowHUD()
    {
        SetCarSelectorAvailable();
        m_UIManager.carSelector.SetActive(true);
    }
}
