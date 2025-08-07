using System;
using System.Collections;
using System.Collections.Generic;
using SFB;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



public class Settings : MonoBehaviour
{

    [SerializeField]
    EyesHandler eyes;

    [SerializeField]
    Slider eyeSpacing;

    [SerializeField]
    Slider heightOffset;

    [SerializeField]
    TMP_Text worldFolderPathText;

    [SerializeField]
    Toggle loadWorldOnBootToggle;

    [SerializeField]
    Toggle standardConnectToggle;

    [SerializeField]
    TMP_Dropdown currentWorldDropdown;

    [SerializeField]
    Slider maxTextureBufferSlider;

    [SerializeField]
    Slider maxMBBufferSlider;

    [SerializeField]
    PopUp popupPrefab;
    public static string worldsOverviewFile = "";
    public static bool loadWorldOnBoot = false;

    [SerializeField]
    GameObject settingsList;

    List<float[]> settingsElements;
    List<GameObject> settingsElementsGameObjects;

    [SerializeField]
    GameObject selector;
    Image selectorImage;
    Color selectorColor;
    [SerializeField]
    Color selectedColor;
    RectTransform selectorRect;


    void Awake()
    {
        LoadValues();
        worldFolderPathText.text = worldsOverviewFile;
        loadWorldOnBootToggle.isOn = loadWorldOnBoot;
        standardConnectToggle.isOn = SerialManager.standardConnect;
        maxTextureBufferSlider.value = TextureManager.maxTexturesToKeep;
        maxMBBufferSlider.value = TextureManager.maxMemoryUsageMB;
    }

    void Start()
    {
        if (eyes == null)
        {
            Debug.LogError("No eyes were given in the Hardware SettingsWrap");
            return;
        }
        eyeSpacing.value = eyes.eyeSpacing;
        heightOffset.value = eyes.heightOffset;
        ToggleView(!isVisible);
    }

    [SerializeField]
    int selectedElement = 0;
    void Update()
    {
        if (loadedElements)
        {
            if (selectedElement < 0 || selectedElement >= settingsElements.Count) return;

            selectorRect.sizeDelta = new Vector2(0, settingsElements[selectedElement][0] * 1.2f);
            selectorRect.anchoredPosition = new Vector2(selectorRect.anchoredPosition.x, settingsElements[selectedElement][1]);
        }
    }

    bool loadedElements = false;

    [SerializeField]
    int totalHeight = 0;
    IEnumerator LoadSettingsElements()
    {
        yield return new WaitForEndOfFrame();
        if (loadedElements) yield break;
        settingsElements = new List<float[]>();
        settingsElementsGameObjects = new List<GameObject>();
        var offset = settingsList.GetComponent<RectTransform>().sizeDelta.y;
        foreach (Transform child in settingsList.transform)
        {
            if (child.GetComponentInChildren<UIInteractable>() == null) continue;
            var trans = child.gameObject.GetComponent<RectTransform>();
            var height = trans.rect.height;
            totalHeight += (int)height;
            var array = new float[2] { height, trans.anchoredPosition.y + offset };
            settingsElements.Add(array);
            settingsElementsGameObjects.Add(child.gameObject);
        }

        selectorRect = selector.GetComponent<RectTransform>();
        selectorImage = selector.GetComponent<Image>();
        selectorColor = selectorImage.color;
        loadedElements = true;
    }

    [SerializeField]
    ScrollRect scrollbar;
    public void MoveSelector(int direction)
    {
        int newPos = selectedElement + direction;
        selectedElement = newPos >= 0 && newPos < settingsElements.Count ? newPos : selectedElement;

        var dimensions = settingsElements[selectedElement];
        var y = 1 - Math.Abs(dimensions[1]) / totalHeight + 0.3f;

        scrollbar.normalizedPosition = new Vector2(0f, y);
        currElement = null;
        selectorImage.color = selectorColor;
    }

    bool isSelected = false;
    UIInteractable currElement;
    public void SelectElement()
    {
        if (!isSelected)
        {
            GameObject element = settingsElementsGameObjects[selectedElement];

            currElement = element.GetComponentInChildren<UIInteractable>();

            selectorImage.color = selectedColor;
            isSelected = true;

            if (currElement != null)
            {
                bool unselect = currElement.Select();

                if (unselect)
                {
                    UnselectElement();
                }
            }
        }
        else
        {
            UnselectElement();
        }
    }

    void UnselectElement()
    {
        isSelected = false;
        selectorImage.color = selectorColor;
        currElement = null;
    }

    public void ShiftElement(int direction)
    {
        if (currElement != null)
        {
            currElement.ShiftElement(direction);
        }
    }



    void OnApplicationQuit()
    {
        SaveValues();
    }

    public void UpdateFilePath(string path)
    {
        worldsOverviewFile = path;
        worldFolderPathText.text = worldsOverviewFile;
    }

    public void UpdateLoadWorldOnBoot(bool value)
    {
        loadWorldOnBoot = value;
    }


    public void SelectNewWorldFolder()
    {
        ExtensionFilter[] extensionList = new[] {
            new ExtensionFilter("XML", "xml")
        };
        string[] path = StandaloneFileBrowser.OpenFilePanel("Wähle Szenen Datei", "", extensionList, false);

        if (path.Length == 1)
        {
            UpdateFilePath(path[0]);
            var popUp = Instantiate(popupPrefab, transform);
            popUp.SetMessage("Starte die App neu, um diese Änderung wirksam zu machen");
        }
    }



    public void ChangeEyeSpacing(float spacing)
    {
        eyes.eyeSpacing = spacing / 10;
    }

    public void ChangeOffsetHeight(float offset)
    {
        eyes.heightOffset = offset;
    }

    public void PopulateWorldDropdown()
    {
        currentWorldDropdown.ClearOptions();
        int indexOfCurrentWorld = 0;
        List<string> options = new List<string>();

        int i = 0;
        foreach (var world in SceneManager.worldsList)
        {
            if (world.Key == SceneManager.currentWorld) indexOfCurrentWorld = i;
            options.Add(world.Key);
            i++;
        }
        currentWorldDropdown.AddOptions(options);
        currentWorldDropdown.value = indexOfCurrentWorld;
    }

    public void UpdateCurrentWorld(int value)
    {
        string world = currentWorldDropdown.options[value].text;
        SceneManager.currentWorld = world;
    }

    private void LoadValues()
    {
        worldsOverviewFile = PlayerPrefs.GetString("scenesOverviewFile", "");
        loadWorldOnBoot = PlayerPrefs.GetInt("loadStartSceenOnBoot", 0) == 1 ? true : false;
        SceneManager.currentWorld = PlayerPrefs.GetString("currentWorld", "");
        SerialManager.standardConnect = PlayerPrefs.GetInt("standardConnect", 0) == 1 ? true : false;
        TextureManager.maxTexturesToKeep = PlayerPrefs.GetInt("maxTexturesToKeep", 16);
        TextureManager.maxMemoryUsageMB = PlayerPrefs.GetInt("maxMemoryUsageMB", 2000);
    }

    private void SaveValues()
    {
        PlayerPrefs.SetString("scenesOverviewFile", worldsOverviewFile);
        PlayerPrefs.SetInt("loadStartSceenOnBoot", loadWorldOnBoot ? 1 : 0);
        PlayerPrefs.SetString("currentWorld", SceneManager.currentWorld);
        PlayerPrefs.SetInt("standardConnect", SerialManager.standardConnect ? 1 : 0);
        PlayerPrefs.SetInt("maxTexturesToKeep", TextureManager.maxTexturesToKeep);
        PlayerPrefs.SetInt("maxMemoryUsageMB", TextureManager.maxMemoryUsageMB);
        PlayerPrefs.Save();
    }

    bool isVisible = false;
    public bool IsVisible { get { return isVisible; } }

    public void CloseView()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        isVisible = false;
        SaveValues();
    }

    public void OpenView()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        isVisible = true;
        StartCoroutine(LoadSettingsElements());
    }

    public void ToggleView()
    {
        ToggleView(isVisible);
    }

    void ToggleView(bool visible)
    {
        if (visible)
        {
            CloseView();
            return;
        }
        OpenView();
    }

}

