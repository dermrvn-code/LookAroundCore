using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    public static Console[] consoleHudManager;

    private static bool consoleActive = false;

    void Start()
    {
        consoleHudManager = FindObjectsByType<Console>(FindObjectsSortMode.None);
    }

    public static void ToggleActivation()
    {
        consoleActive = !consoleActive;
        foreach (var chm in consoleHudManager)
        {
            chm.SetActive(consoleActive);
        }

        if (consoleActive) Log("Console activated");
    }

    public static void Log(string text)
    {
        if (consoleActive)
        {
            foreach (var chm in consoleHudManager)
            {
                chm.Log(text);
            }
        }
    }


    public static void LogValues(int rotation, bool interaction1, bool interaction2, int zoom)
    {
        if (consoleActive)
        {
            foreach (var chm in consoleHudManager)
            {
                chm.LogValues(rotation, interaction1, interaction2, zoom);
            }
        }
    }
}
