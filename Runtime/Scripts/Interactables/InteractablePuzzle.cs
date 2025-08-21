
using TMPro;
using UnityEngine;

public class InteractablePuzzle : Interactable
{
    public Texture2D texture;

    Animation anim;
    ParticleSystem particles;
    PuzzleManager puzzleManager;

    public bool active = true;

    MeshRenderer meshRenderer;
    public int id;

    public override void Setup()
    {
        puzzleManager = FindFirstObjectByType<PuzzleManager>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        anim = GetComponentInChildren<Animation>();
        particles = GetComponentInChildren<ParticleSystem>();
        SetImage(texture);
        Unhighlight();

        OnInteract.AddListener(() =>
        {
            CollectPiece();
        });
    }

    void Update()
    {
        meshRenderer.enabled = puzzleManager.CanCollect(id);
    }

    public void SetImage(Texture2D newTexture)
    {
        texture = newTexture;
        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material = new Material(meshRenderer.material);
            meshRenderer.material.mainTexture = texture;

            Vector2 logoSize = new Vector2(texture.width, texture.height);
            float maxDimension = Mathf.Max(logoSize.x, logoSize.y);
            float aspectWidth = logoSize.x / maxDimension;
            float aspectHeight = logoSize.y / maxDimension;

            meshRenderer.material.SetFloat("_TexWidth", aspectWidth);
            meshRenderer.material.SetFloat("_TexHeight", aspectHeight);
        }
    }

    public override void Highlight()
    {
        anim.enabled = true;
    }
    public override void Unhighlight()
    {
        anim.enabled = false;
    }

    void CollectPiece()
    {
        if (!active) return;
        if (puzzleManager.CanCollect(id))
        {
            puzzleManager.CollectPiece(id);
            meshRenderer.enabled = false;
            particles.Play();
        }
    }

}