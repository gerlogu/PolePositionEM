using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TimerAnimManager : MonoBehaviour
{
    [SerializeField] Text timerText;

    public void SetText(string text)
    {
        timerText.text = text;
    }
}
