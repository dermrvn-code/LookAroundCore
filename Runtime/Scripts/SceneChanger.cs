using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public abstract class SceneChanger : MonoBehaviour
{
    public GameObject particlesGameobject;
    ParticleSystem particles;

    [SerializeField] MeshRenderer domeRenderer;
    [SerializeField] Material videoMaterial;
    [SerializeField] Material photoMaterial;
    [SerializeField] Texture mainScreenImage;
    [SerializeField] GameObject sceneElementsContainer;
    [SerializeField] VideoPlayer videoPlayer;

    protected SceneManager sceneManager;
    protected InteractionHandler interactionHandler;
    protected TextureManager textureManager;
    protected LogoLoadingOverlay loadingOverlay;
    protected SpriteManager spriteManager;
    protected ModelManager modelManager;

    Scene currentScene;

    [SerializeField] TMP_Text textPrefab;
    [SerializeField] GameObject textboxPrefab;
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] GameObject spritePrefab;

    public Sprite info, warning, question, play;

    public static string[] actionTypes = { "toScene" };

    void Start()
    {
        sceneManager = FindFirstObjectByType<SceneManager>();
        interactionHandler = FindFirstObjectByType<InteractionHandler>();
        textureManager = FindFirstObjectByType<TextureManager>();
        modelManager = FindFirstObjectByType<ModelManager>();
        loadingOverlay = FindFirstObjectByType<LogoLoadingOverlay>();
        spriteManager = FindFirstObjectByType<SpriteManager>();

        particlesGameobject.SetActive(true);
        particles = particlesGameobject.GetComponent<ParticleSystem>();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ToMainScene()
    {
        photoMaterial.mainTexture = mainScreenImage;
        photoMaterial.mainTextureOffset = Vector2.zero;
        SwitchToFoto();
    }

    public void ToStartScene(bool animate = true)
    {
        bool foundScene = false;
        foreach (var scene in sceneManager.sceneList)
        {
            if (scene.Value.IsStartScene)
            {
                foundScene = true;
                if (animate)
                    SwitchSceneAnimation(scene.Value);
                else
                    SwitchScene(scene.Value);
            }
        }
        if (!foundScene)
            Debug.LogWarning("There is no start scene specified");
    }

    void SwitchToVideo()
    {
        domeRenderer.material = videoMaterial;
    }

    void SwitchToFoto()
    {
        domeRenderer.material = photoMaterial;
    }

    public void SwitchSceneAnimation(Scene scene, int index = -1)
    {
        if (scene == null || scene == currentScene) return;

        if (index == -1)
        {
            TransitionParticles(sceneLoaded =>
                SwitchScene(scene, () => sceneLoaded?.Invoke()));
        }
        else
        {
            TransitionLogo(sceneLoaded =>
                SwitchScene(scene, () => sceneLoaded?.Invoke()), logoIndex: index);
        }
    }

    public void SwitchScene(Scene scene, Action onLoaded = null)
    {
        if (scene == null || scene == currentScene) return;

        currentScene = scene;
        LoadSceneElements(scene.SceneElements);
        if (interactionHandler != null)
            interactionHandler.updateElementsNextFrame = true;

        try
        {
            if (!File.Exists(scene.Source))
            {
                Debug.LogWarning("Scene media " + scene.Source + " does not exist");
                return;
            }

            if (scene.Type == Scene.MediaType.Video)
            {
                SwitchToVideo();
                videoMaterial.mainTextureOffset = new Vector2(scene.XOffset, scene.YOffset);
                videoPlayer.url = scene.Source;
                onLoaded?.Invoke();
            }
            else if (scene.Type == Scene.MediaType.Photo)
            {
                StartCoroutine(textureManager.GetTexture(scene.Source, texture =>
                {
                    photoMaterial.mainTexture = texture;
                    photoMaterial.mainTextureOffset = new Vector2(scene.XOffset, scene.YOffset);
                    SwitchToFoto();
                    onLoaded?.Invoke();
                }));
            }
            else
            {
                Debug.LogWarning("Scene type not supported");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error while switching to scene " + scene.Name);
            Debug.LogError(e.Message);
        }
    }

    public void LoadSceneElements(List<SceneElement> sceneElements)
    {
        modelManager.HideAllModels();

        var children = new List<GameObject>();
        foreach (Transform child in sceneElementsContainer.transform)
            children.Add(child.gameObject);

        if (Application.isPlaying)
            children.ForEach(Destroy);
        else
            children.ForEach(DestroyImmediate);

        foreach (var sceneElement in sceneElements)
        {
            switch (sceneElement)
            {
                case SceneElementText text: LoadTextElement(text); break;
                case SceneElementTextbox textbox: LoadTextboxElement(textbox); break;
                case SceneElementArrow arrow: LoadArrow(arrow); break;
                case SceneElementModel model: LoadModel(model); break;
                case SceneElementSprite sprite: LoadSprite(sprite); break;
            }
        }
    }

    public void LoadTextElement(SceneElementText sceneElement)
    {
        var text = Instantiate(textPrefab, sceneElementsContainer.transform);

        ColorUtility.TryParseHtmlString(sceneElement.color, out Color textColor);

        text.name = sceneElement.text;
        text.text = sceneElement.text;
        text.color = textColor;

        var dp = text.GetComponent<DomePosition>();
        dp.position.x = sceneElement.x;
        dp.position.y = sceneElement.y;
        dp.distance = sceneElement.distance;
        dp.xRotOffset = sceneElement.xRotationOffset;

        var interactable = text.GetComponent<Interactable>();
        interactable.OnInteract.AddListener(() => ActionParser(sceneElement.action));
    }

    public void LoadTextboxElement(SceneElementTextbox sceneElement)
    {
        var text = Instantiate(textboxPrefab, sceneElementsContainer.transform);
        var tmptext = text.GetComponentInChildren<TMP_Text>();
        var spriteRenderer = text.GetComponentInChildren<SpriteRenderer>();
        var meshRenderer = text.GetComponentInChildren<MeshRenderer>();
        var dp = text.GetComponent<DomePosition>();

        ColorUtility.TryParseHtmlString(sceneElement.color, out Color bgColor);

        float luminance = 0.299f * bgColor.r + 0.587f * bgColor.g + 0.114f * bgColor.b;
        Color bestTextColor = luminance > 0.7f ? Color.black : Color.white;

        tmptext.text = sceneElement.text;
        tmptext.color = bestTextColor;
        spriteRenderer.color = bestTextColor;

        dp.position.x = sceneElement.x;
        dp.position.y = sceneElement.y;
        dp.distance = sceneElement.distance;
        dp.xRotOffset = sceneElement.xRotationOffset;
        meshRenderer.material.color = bgColor;

        Sprite sprite = sceneElement.icon switch
        {
            "info" => info,
            "warning" => warning,
            "question" => question,
            "play" => play,
            _ => null
        };
        if (sprite != null)
            spriteRenderer.sprite = sprite;
    }

    public void LoadArrow(SceneElementArrow sceneElement)
    {
        var arrow = Instantiate(arrowPrefab, sceneElementsContainer.transform);

        arrow.GetComponentInChildren<InteractableArrow>().SetRotation(sceneElement.rotation);

        var dp = arrow.GetComponent<DomePosition>();
        dp.position.x = sceneElement.x;
        dp.position.y = sceneElement.y;
        dp.distance = sceneElement.distance;
        dp.xRotOffset = sceneElement.xRotationOffset;

        var interactableArrow = arrow.GetComponent<InteractableArrow>();
        interactableArrow.OnInteract.AddListener(() => ActionParser(sceneElement.action));

        interactableArrow.color = ColorUtility.TryParseHtmlString(sceneElement.color, out Color unityColor) ? unityColor : Color.white;
    }

    public void LoadModel(SceneElementModel sceneElement)
    {
        var dp = modelManager.DisplayModel(sceneElement.modelName);
        if (dp == null) return;

        dp.position.x = sceneElement.x;
        dp.position.y = sceneElement.y;
        dp.distance = sceneElement.distance;
        dp.xRotOffset = sceneElement.xRotationOffset;

        var modelTransform = dp.GetComponent<ModelTransform>();
        modelTransform.rotation.x = sceneElement.xRotation;
        modelTransform.rotation.y = sceneElement.yRotation;
        modelTransform.rotation.z = sceneElement.zRotation;
        modelTransform.scale = sceneElement.scale;

        var interactableModel = dp.GetComponent<InteractableModel>();
        interactableModel.OnInteract.AddListener(() => ActionParser(sceneElement.action));
    }

    public void LoadSprite(SceneElementSprite sceneElement)
    {
        var texture = spriteManager.GetSprite(sceneElement.index);
        if (texture == null) return;

        var sprite = Instantiate(spritePrefab, sceneElementsContainer.transform).GetComponent<InteractableSprite>();
        sprite.texture = texture;

        var dp = sprite.GetComponent<DomePosition>();
        dp.position.x = sceneElement.x;
        dp.position.y = sceneElement.y;
        dp.distance = sceneElement.distance;
        dp.xRotOffset = sceneElement.xRotationOffset;

        sprite.OnInteract.AddListener(() => ActionParser(sceneElement.action));
    }

    public void ActionParser(string action)
    {
        string pattern = @"toScene\(([^,]*?)(?:,(-?\d))*\)";
        Match match = Regex.Match(action, pattern);
        if (match.Success)
        {
            string sceneName = match.Groups[1].Value;
            int animationIndex = -1;
            if (match.Groups.Count > 2 && match.Groups[2].Success)
                int.TryParse(match.Groups[2].Value.Trim(), out animationIndex);

            if (sceneManager.sceneList.TryGetValue(sceneName, out Scene scene) && scene != null)
                SwitchSceneAnimation(scene, animationIndex);
        }
    }

    public void TransitionLogo(Action<Action> sceneLoaded, int logoIndex)
    {
        StartCoroutine(_FadeIn(sceneLoaded, logoIndex));
    }

    public void TransitionParticles(Action<Action> sceneLoaded)
    {
        StartCoroutine(_StartParticles(sceneLoaded));
    }

    private IEnumerator _StartParticles(Action<Action> sceneLoaded)
    {
        particles.Play();
        yield return new WaitForSeconds(2f);
        sceneLoaded.Invoke(() => StartCoroutine(_StopParticles()));
    }

    private IEnumerator _StopParticles()
    {
        yield return new WaitForSeconds(0.5f);
        particles.Stop();
    }

    public IEnumerator _FadeIn(Action<Action> sceneLoaded, int logoIndex)
    {
        spriteManager.SetLogoFromIndex(logoIndex);
        loadingOverlay.FadeIn();
        yield return new WaitForSeconds(2f);
        sceneLoaded.Invoke(() => StartCoroutine(_FadeOut()));
    }

    private IEnumerator _FadeOut()
    {
        yield return new WaitForSeconds(0.5f);
        loadingOverlay.FadeOut();
    }
}
