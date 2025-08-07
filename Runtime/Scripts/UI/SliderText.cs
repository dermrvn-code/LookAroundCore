using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderText : MonoBehaviour
{


    public Slider slider;
    public TMP_Text text;


    private void Start()
    {
        UpdateText(slider.value);
    }

    public void UpdateText(float value)
    {
        text.text = Math.Round(value, 2).ToString();
    }



}
