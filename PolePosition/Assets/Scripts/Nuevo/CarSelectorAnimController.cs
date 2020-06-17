using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSelectorAnimController : MonoBehaviour
{
    [SerializeField] private UIManager m_UIManager;

    public void SetCarSelectorAvailable()
    {
        m_UIManager.canSelect = true;
    }

    public void ShowHUD()
    {
        SetCarSelectorAvailable();
        m_UIManager.carSelector.SetActive(true);
    }
}
