using UnityEngine;

/// <summary>
/// Clase coche que contiene las características del mismo.
/// </summary>
[CreateAssetMenu(fileName = "New car", menuName = "Car")]
public class Car : ScriptableObject
{
    [Tooltip("Materiales del vehículo")]
    public Material[] carMaterials;
}
