using UnityEngine;

public class HardwareEmulator : MonoBehaviour
{
    public EyesHandler eyes;
    public Settings settings;
    public InteractionHandler interaction;
    public SceneChanger sc;

    bool isSceneBuilder = false;
    void Start()
    {
        sc = FindFirstObjectByType<SceneChanger>();
        if (eyes == null) Debug.LogError("No eyes were given in the Hardware Emulator");
        if (settings == null)
        {
            if (!SceneManager.isSceneBuilder())
            {
                Debug.LogError("No settings were given in the Hardware Emulator");
                return;
            }
            else
            {
                Debug.LogWarning("No settings were given in the Hardware Emulator, but it's a Scene Builder");
            }
            isSceneBuilder = true;
        }
    }

    void Update()
    {
        bool settingsVisible = false;
        if (!isSceneBuilder)
        {
            settingsVisible = settings.IsVisible;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (!settingsVisible)
            {
                eyes.LeftMove();
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (!settingsVisible)
            {
                eyes.RightMove();
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (settingsVisible)
            {
                settings.ShiftElement(-1);
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (settingsVisible)
            {
                settings.ShiftElement(1);
            }
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (!settingsVisible)
            {
                eyes.ZoomIn();
            }
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            if (!settingsVisible)
            {
                eyes.ZoomOut();
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (settingsVisible)
            {
                settings.MoveSelector(1);
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (settingsVisible)
            {
                settings.MoveSelector(-1);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            settings.ToggleView();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (settingsVisible)
            {
                settings.SelectElement();
            }
            else
            {
                interaction.Interact();
            }
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (!settingsVisible)
            {
                sc.ToStartScene();
            }
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            DebugConsole.ToggleActivation();
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            eyes.ToggleSplitScreen();
        }
    }
}
