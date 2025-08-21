using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Siccity.GLTFUtility;
using UnityEngine.Events;
using System.Linq;

public struct Model
{
    public string name;
    public string path;
    public GameObject gameobject;
    public Texture2D preview;

    public bool used;

    public Model(string name, string path, GameObject gameobject)
    {
        this.name = name;
        this.path = path;
        this.gameobject = gameobject;
        preview = null;
        used = false;
    }
}

public abstract class ModelManagerBase : MonoBehaviour
{
    [SerializeField]
    protected int maxModels = 8;

    [SerializeField]
    protected Dictionary<string, Model> loadedModels = new Dictionary<string, Model>();

    [SerializeField]
    protected GameObject sceneElementsContainer;

    [SerializeField]
    protected GameObject siding;

    [SerializeField]
    protected Renderer domeRenderer;


    public virtual void Start()
    {
        if (domeRenderer == null)
        {
            Debug.LogWarning("No Renderer found on dome.");
        }

        if (sceneElementsContainer == null)
        {
            Debug.LogWarning("sceneElementsContainer or dome is not assigned.");
        }
    }

    public string GetFirstModel()
    {
        foreach (var model in loadedModels.Values)
        {
            if (model.name != null && !model.used)
            {
                return model.name;
            }
        }
        return null;
    }


    [SerializeField]
    protected InteractableModel containerPrefab;
    public GameObject DisplayModel(string modelName)
    {
        if (loadedModels.TryGetValue(modelName, out Model model))
        {
            var container = Instantiate(containerPrefab, sceneElementsContainer.transform);

            var animContainer = container.GetComponent<InteractableModel>().elementContainer;
            model.gameobject.SetActive(true);
            model.gameobject.transform.SetParent(animContainer.transform, false);

            return container.gameObject;
        }

        Debug.LogWarning("Model not found in loaded models: " + modelName);
        return null;
    }

    public virtual void HideModel(string modelName)
    {
        if (!loadedModels.TryGetValue(modelName, out Model model))
        {
            Debug.LogWarning("Model not found in loaded models: " + modelName);
            return;
        }
        model.gameobject.transform.SetParent(siding.transform, false);
        model.gameobject.SetActive(false);

    }

    public void HideAllModels(string exceptModelName = null)
    {
        foreach (var model in loadedModels)
        {
            if (model.Key != exceptModelName && model.Value.gameobject != null)
            {
                model.Value.gameobject.transform.SetParent(siding.transform, false);
                model.Value.gameobject.SetActive(false);
            }
        }
    }

    public void HideAllModels()
    {
        foreach (var model in loadedModels.Values)
        {
            if (model.gameobject != null)
            {
                model.gameobject.transform.SetParent(siding.transform, false);
                model.gameobject.SetActive(false);
            }
        }
    }

    public string[] GetModelNames(int? maxModels = null)
    {
        return loadedModels.Keys.Take(maxModels.GetValueOrDefault(this.maxModels)).ToArray();
    }

    public void LoadModel(string filepath, string modelName, UnityAction<GameObject, Texture2D> onLoaded = null)
    {
        Importer.ImportGLTFAsync(filepath, new ImportSettings(), (GameObject result, AnimationClip[] clips) =>
        {
            foreach (var meshFilter in result.GetComponentsInChildren<MeshFilter>())
            {
                var collider = meshFilter.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = meshFilter.sharedMesh;
            }

            result.name = modelName;
            NormalizeModel(result);

            OnModelLoaded(modelName, result, filepath, onLoaded);
        });
    }

    public virtual void OnModelLoaded(string modelName, GameObject result, string filePath, UnityAction<GameObject, Texture2D> onLoaded = null)
    {
        IntegrateModel(modelName, result, filePath);
        onLoaded?.Invoke(result, null);
        Debug.Log($"Model {modelName} loaded from {filePath}");
    }


    public void UnloadModel(string modelName, Action<Model> onUnloaded = null)
    {
        if (loadedModels.ContainsKey(modelName))
        {
            loadedModels.TryGetValue(modelName, out Model model);
            loadedModels.Remove(modelName);
            if (model.gameobject != null)
            {
                Destroy(model.gameobject);
            }
            Debug.Log($"Model {modelName} unloaded.");
            onUnloaded?.Invoke(model);
            return;
        }
        Debug.LogWarning("Model not found in loaded models: " + modelName);
    }



    float realismFactor = 0.2f;
    protected void NormalizeModel(GameObject model)
    {
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        Renderer resultRenderer = model.GetComponent<Renderer>();
        if (resultRenderer == null)
        {
            resultRenderer = model.GetComponentInChildren<Renderer>();
        }

        if (resultRenderer == null)
        {
            Debug.LogWarning("No Renderer found on result to calculate size.");
            return;
        }

        Bounds meshBounds = new Bounds();
        bool hasBounds = false;
        foreach (var meshFilter in model.GetComponentsInChildren<MeshFilter>())
        {
            if (meshFilter.sharedMesh != null)
            {
                if (!hasBounds)
                {
                    meshBounds = meshFilter.sharedMesh.bounds;
                    meshBounds.center = meshFilter.transform.TransformPoint(meshBounds.center);
                    hasBounds = true;
                }
                else
                {
                    Bounds transformedBounds = meshFilter.sharedMesh.bounds;
                    transformedBounds.center = meshFilter.transform.TransformPoint(transformedBounds.center);
                    meshBounds.Encapsulate(transformedBounds);
                }
            }
        }
        Vector3 resultSize = hasBounds ? meshBounds.size : Vector3.zero;
        Vector3 domeSize = domeRenderer.bounds.size;

        float resultMax = Mathf.Max(resultSize.x, resultSize.y, resultSize.z);

        if (resultMax == 0)
        {
            Debug.LogWarning("Result size is zero, cannot scale.");
            return;
        }

        float scaleFactor;
        if (resultMax == resultSize.x)
        {
            scaleFactor = domeSize.x / resultMax;
        }
        else if (resultMax == resultSize.y)
        {
            scaleFactor = domeSize.y / resultMax;
        }
        else
        {
            scaleFactor = domeSize.z / resultMax;
        }
        scaleFactor = scaleFactor * realismFactor; // realistic scaling
        model.transform.localScale = Vector3.one * scaleFactor;
    }

    protected void IntegrateModel(string modelName, GameObject result, string filepath)
    {
        if (loadedModels.Count >= maxModels)
        {
            Debug.LogWarning("Maximum number of models loaded. Cannot load more.");
            return;
        }

        if (loadedModels.ContainsKey(modelName))
        {
            Debug.LogWarning("Model already loaded: " + result.name);
            return;
        }

        result.transform.SetParent(siding.transform, false);
        result.gameObject.SetActive(false);

        Model model = new Model(modelName, filepath, result);

        loadedModels.Add(modelName, model);
    }

    public virtual void UnloadAllModels()
    {
        foreach (var model in loadedModels.Values)
        {
            if (model.gameobject != null)
            {
                Destroy(model.gameobject);
            }
        }
        loadedModels.Clear();
        Debug.Log("All models unloaded.");
    }

    void OnDestroy()
    {
        UnloadAllModels();
    }

}
