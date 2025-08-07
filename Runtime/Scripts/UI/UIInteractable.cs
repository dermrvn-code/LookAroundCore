using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class UIInteractable : MonoBehaviour
{

    public enum InteractionType
    {
        Button,
        Slider,
        Toggle,
        Dropdown
    }
    public InteractionType interactionType;

    void Start()
    {
        if (gameObject.GetComponent<Button>() != null)
        {
            interactionType = InteractionType.Button;
        }
        else if (gameObject.GetComponent<Slider>() != null)
        {
            interactionType = InteractionType.Slider;
        }
        else if (gameObject.GetComponent<Toggle>() != null)
        {
            interactionType = InteractionType.Toggle;
        }
        else if (gameObject.GetComponent<TMP_Dropdown>() != null)
        {
            interactionType = InteractionType.Dropdown;
        }
    }

    public bool Select()
    {
        switch (interactionType)
        {
            case InteractionType.Button:
                gameObject.GetComponent<Button>().onClick.Invoke();
                return true;
            case InteractionType.Slider:
                return false;
            case InteractionType.Toggle:
                Toggle tgl = gameObject.GetComponent<Toggle>();
                tgl.isOn = !tgl.isOn;
                return true;
            case InteractionType.Dropdown:
                return false;
        }
        return true;
    }

    public void ShiftElement(int direction)
    {
        if (interactionType == InteractionType.Slider)
        {
            Slider sldr = gameObject.GetComponent<Slider>();
            sldr.value = sldr.value + direction;
        }
        else if (interactionType == InteractionType.Dropdown)
        {
            TMP_Dropdown dropdown = gameObject.GetComponent<TMP_Dropdown>();
            dropdown.value = dropdown.value + direction;
        }
    }
}
