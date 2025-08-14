using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public abstract class SceneManagerBase : MonoBehaviour
{

    protected SceneChangerBase sceneChanger;
    protected TextureManager textureManager;
    protected ModelManagerBase modelManager;

    protected XDocument worldOverview;
    protected XDocument scenesOverview;
    protected ProgressBarBase progressBar;
    protected SpriteManager spriteManager;
    protected LogoLoadingOverlay logoLoadingOverlay;
    public Dictionary<string, Scene> sceneList = new Dictionary<string, Scene>();

    public virtual void Start()
    {
        sceneChanger = FindFirstObjectByType<SceneChangerBase>();
        textureManager = FindFirstObjectByType<TextureManager>();
        modelManager = FindFirstObjectByType<ModelManagerBase>();
        progressBar = FindFirstObjectByType<ProgressBarBase>();
        spriteManager = FindFirstObjectByType<SpriteManager>();

        logoLoadingOverlay = FindFirstObjectByType<LogoLoadingOverlay>();
    }

    List<string> texturePaths;
    Dictionary<string, string> modelPaths = new Dictionary<string, string>();
    Dictionary<int, string> spritePaths = new Dictionary<int, string>();

    string currentScenesOverviewPath;
    public void LoadScenesOverview(string path, Action onComplete)
    {
        if (currentScenesOverviewPath == path)
        {
            sceneChanger.ToStartScene();
            return;
        }
        currentScenesOverviewPath = path;
        texturePaths = new List<string>();
        textureManager.ReleaseAllTextures();

        modelPaths = new Dictionary<string, string>();
        modelManager.UnloadAllModels();

        sceneList = new Dictionary<string, Scene>();

        if (!File.Exists(currentScenesOverviewPath)) Debug.LogWarning("The scene overview file does not exist: " + currentScenesOverviewPath);
        scenesOverview = XDocument.Load(currentScenesOverviewPath);

        LoadScenes(currentScenesOverviewPath);
        LoadLogos(currentScenesOverviewPath);
        LoadModels(currentScenesOverviewPath);
        LoadSprites(currentScenesOverviewPath);

        int maxLoadingSteps = texturePaths.Count + modelPaths.Count + spritePaths.Count;

        progressBar.OnFull(() =>
        {
            onComplete?.Invoke();
        });

        StartCoroutine(textureManager.LoadAllTextures(texturePaths,
        (float progress, string path) => // onProgress
        {
            progressBar.UpdateBarIncreaseSteps(maxLoadingSteps, Path.GetFileName(path));
        },
        () => // onComplete
        {
            Debug.Log("Textures preloaded!");
        }));

        foreach (var spritePath in spritePaths)
        {
            spriteManager.LoadSprite(spritePath.Key, spritePath.Value);
            progressBar.UpdateBarIncreaseSteps(maxLoadingSteps, Path.GetFileName(spritePath.Value));
        }

        foreach (var model in modelPaths)
        {
            string modelName = model.Key;
            string modelPath = model.Value;

            modelManager.LoadModel(modelPath, modelName, (GameObject result, Texture2D texture) =>
            {
                progressBar.UpdateBarIncreaseSteps(maxLoadingSteps, Path.GetFileName(modelPath));
            });
        }

    }


    void LoadScenes(string sceneOverviewPath)
    {
        var scenesList = scenesOverview.Root.Element("Scenes");
        var scenes = scenesList.Descendants("Scene");

        int counter = 0;
        foreach (var scene in scenes)
        {
            string scenePath = scene.Attribute("path").Value;
            string sceneName = scene.Attribute("name").Value;

            var startScene = scene.Attribute("startScene");
            bool isStartScene = false;
            if (startScene != null)
            {
                if (startScene.Value.ToLower() == "true") isStartScene = true;
            }

            string sceneFolder = Path.GetDirectoryName(sceneOverviewPath);
            Scene s = LoadScene(sceneName, sceneFolder, scenePath, isStartScene);

            if (s.Type == Scene.MediaType.Photo)
            {
                if (s.IsStartScene)
                {
                    texturePaths.Insert(0, s.Source);
                }
                else
                {
                    texturePaths.Add(s.Source);
                }
            }

            counter++;
        }
    }

    void LoadLogos(string sceneOverviewPath)
    {
        var logoList = scenesOverview.Root.Element("Logos");
        if (logoList != null)
        {
            var logos = logoList.Descendants("Logo");
            foreach (var logo in logos)
            {
                string logoSource = logo.Attribute("source").Value;
                string id_str = logo.Attribute("id").Value;
                string backgroundColor = logo.Attribute("backgroundColor")?.Value ?? "";

                if (int.TryParse(id_str, out int id))
                {
                    string logoPath = Path.Combine(Path.GetDirectoryName(sceneOverviewPath), logoSource);
                    if (File.Exists(logoPath))
                    {
                        spriteManager.LoadLogo(id, logoPath, backgroundColor);
                    }
                    else
                    {
                        Debug.LogWarning("Logo file does not exist: " + logoPath);
                    }
                }
            }
        }
    }

    void LoadSprites(string scenesOverviewPath)
    {
        var spritesList = scenesOverview.Root.Element("Sprites");
        if (spritesList != null)
        {
            var sprites = spritesList.Descendants("Sprite");
            foreach (var sprite in sprites)
            {
                string spriteSource = sprite.Attribute("source").Value;
                int index = int.Parse(sprite.Attribute("id").Value);

                string spritePath = Path.Combine(Path.GetDirectoryName(scenesOverviewPath), spriteSource);
                if (File.Exists(spritePath))
                {
                    spritePaths.Add(index, spritePath); ;
                }
                else
                {
                    Debug.LogWarning("Sprite file does not exist: " + spritePath);
                }
            }
        }
    }

    void LoadModels(string scenesOverviewPath)
    {
        var modelsList = scenesOverview.Root.Element("Models");

        if (modelsList != null)
        {
            var models = modelsList.Descendants("Model");

            foreach (var model in models)
            {
                string modelSource = model.Attribute("source").Value;
                string modelName = model.Attribute("name").Value;

                string modelPath = Path.Combine(Path.GetDirectoryName(scenesOverviewPath), modelSource);

                if (File.Exists(modelPath))
                {
                    modelPaths.Add(modelName, modelPath);
                    continue;
                }
                Debug.LogWarning("Model file does not exist: " + modelPath);
            }
        }
    }

    Scene LoadScene(string sceneName, string mainFolder, string scenePath, bool isStartScene)
    {
        var sceneXML = XDocument.Load(mainFolder + "/" + scenePath);

        var sceneTag = sceneXML.Element("Scene");
        string type = sceneTag.Attribute("type").Value;
        string source = sceneTag.Attribute("source").Value;


        float xOffset = 0;
        float yOffset = 0;
        if (sceneTag.Attribute("xOffset") != null)
        {
            xOffset = float.Parse(sceneTag.Attribute("xOffset").Value);
        }
        if (sceneTag.Attribute("yOffset") != null)
        {
            yOffset = float.Parse(sceneTag.Attribute("yOffset").Value);
        }

        string sceneFolder = Path.GetDirectoryName(mainFolder + "/" + scenePath);
        source = Path.Combine(sceneFolder, source);


        var elements = sceneTag.Descendants("Element");

        var sceneElements = new Dictionary<int, SceneElement>();

        int elementId = 0;
        foreach (var element in elements)
        {
            string elementType = element.Attribute("type").Value.ToLower();

            string text = element.Value.Trim();
            if (text == "")
            {
                text = "No Text given";
            }

            int x = int.Parse(element.Attribute("x").Value);
            int y = int.Parse(element.Attribute("y").Value);

            int distance = TryGetAttributeInt(element, "distance", 10);


            int xRotationOffset = TryGetAttributeInt(element, "xRotationOffset", 0);


            SceneElement se;
            if (elementType == "text")
            {
                string action = TryGetAttributeString(element, "action", "");
                se = new SceneElementText(
                    text: text,
                    x: x, y: y,
                    distance: distance,
                    xRotationOffset: xRotationOffset,
                    action: action
                );
            }
            else if (elementType == "textbox")
            {
                string icon = element.Attribute("icon").Value;
                se = new SceneElementTextbox(
                    text: text, icon: icon,
                    x: x, y: y,
                    distance: distance,
                    xRotationOffset: xRotationOffset
                );
            }
            else if (elementType == "directionarrow")
            {

                string action = TryGetAttributeString(element, "action", "");
                int rotation = TryGetAttributeInt(element, "rotation", 0);

                string color = "";
                if (element.Attribute("color") != null)
                {
                    color = element.Attribute("color").Value;
                }

                se = new SceneElementArrow(
                    x: x, y: y,
                    distance: distance,
                    xRotationOffset: xRotationOffset,
                    rotation: rotation,
                    color: color, action: action
                );

            }
            else if (elementType == "model")
            {
                string name = element.Attribute("name").Value;

                string action = TryGetAttributeString(element, "action", "");
                int rotationX = TryGetAttributeInt(element, "rotationX", 0);
                int rotationY = TryGetAttributeInt(element, "rotationY", 0);
                int rotationZ = TryGetAttributeInt(element, "rotationZ", 0);
                int scale = TryGetAttributeInt(element, "scale", 1);


                se = new SceneElementModel(
                    modelName: name,
                    x: x, y: y,
                    distance: distance,
                    xRotationOffset: xRotationOffset,
                    action: action,
                    xRotation: rotationX,
                    yRotation: rotationY,
                    zRotation: rotationZ,
                    scale: scale
                );
            }
            else if (elementType == "sprite")
            {
                string path = TryGetAttributeString(element, "source", "");
                int id = TryGetAttributeInt(element, "id", -1);
                string action = TryGetAttributeString(element, "action", "");

                se = new SceneElementSprite(
                    x: x, y: y,
                    distance: distance,
                    xRotationOffset: xRotationOffset,
                    path: path,
                    index: id,
                    action: action
                );
            }
            else
            {
                Debug.Log("Element doesnt match any type : " + elementType);
                se = null;
            }
            if (se != null)
            {
                sceneElements.Add(elementId, se);
                elementId++;
            }
        }
        Scene sceneObj = new Scene(type == "video" ? Scene.MediaType.Video : Scene.MediaType.Photo, sceneName, source, sceneElements, isStartScene, xOffset, yOffset);

        sceneList.Add(sceneName, sceneObj);
        return sceneObj;
    }

    int TryGetAttributeInt(XElement element, string attributeName, int defaultValue)
    {
        if (element.Attribute(attributeName) != null)
        {
            return int.Parse(element.Attribute(attributeName).Value);
        }
        return defaultValue;
    }

    string TryGetAttributeString(XElement element, string attributeName, string defaultValue)
    {
        if (element.Attribute(attributeName) != null)
        {
            return element.Attribute(attributeName).Value;
        }
        return defaultValue;
    }

    public Scene GetStartScene()
    {
        foreach (var scene in sceneList.Values)
        {
            if (scene.IsStartScene)
            {
                return scene;
            }
        }
        return null;
    }

    public void SetStartScene(string sceneName = "", string sceneNameAvoid = "")
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            if (sceneList.ContainsKey(sceneName))
            {
                foreach (var scene in sceneList.Values)
                {
                    scene.SetStartScene(false); // Reset all scenes
                }
                sceneList[sceneName].SetStartScene(true); // Set the specified scene as start scene
            }
            else
            {
                Debug.LogWarning("Scene not found: " + sceneName);
            }
        }
        else
        {
            foreach (var scene in sceneList.Values)
            {
                scene.SetStartScene(false); // Reset all scenes
            }

            sceneList.FirstOrDefault(x => x.Key != sceneNameAvoid).Value?.SetStartScene(true);
        }


    }
    void OnDestroy()
    {
        // Release all textures when done
        textureManager.ReleaseAllTextures();
    }

}
