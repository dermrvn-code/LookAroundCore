using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Console : MonoBehaviour
{
    public TMP_Text consoleText;

    public TMP_Text hudText;

    void Start()
    {
        consoleText = GetComponent<TMP_Text>();
        hudText = transform.GetChild(0).GetComponent<TMP_Text>();
    }

    public void SetActive(bool status)
    {
        consoleText.enabled = status;
        hudText.enabled = status;
    }

    public void Log(string text)
    {
        var clearAfterLines = 10;
        var oldText = consoleText.text;
        var oldLines = oldText.Split("\n");
        if (oldLines.Length > clearAfterLines)
        {
            oldLines = oldLines.Skip(1).ToArray();
            oldLines[0] = "[...]";
            oldText = string.Join("\n", oldLines);
        }
        consoleText.text = oldText + "\n" + "[" + DateTime.Now.ToString() + "] " + text;
    }


    public void LogValues(int rotation, bool interaction1, bool interaction2, int zoom)
    {
        hudText.text = "Rotation: " + rotation + "\n";
        hudText.text += "Zoom: " + zoom + "\n";
        hudText.text += "Interaction 1: " + interaction1 + "\n";
        hudText.text += "Interaction 2: " + interaction2;
    }
}
