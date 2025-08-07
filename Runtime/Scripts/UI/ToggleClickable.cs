using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class ToggleClickable : MonoBehaviour
{
    public Toggle toggle;

    public void ToggleToggle()
    {
        toggle.isOn = !toggle.isOn;
    }
}
