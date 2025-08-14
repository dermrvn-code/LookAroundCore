using UnityEngine;

public class PuzzleDisplay : MonoBehaviour
{
    Camera targetCamera;
    float distanceFromCamera = 0.5f;
    public float borderPadding = 0.18f;

    public Material puzzleMaterial;
    public Material puzzleMaterialMissing;
    public Texture2D image;



    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        PositionAtTopBorder();

        FitImage(puzzleMaterial, image);
        FitImage(puzzleMaterialMissing, image);
    }

    void FitImage(Material material, Texture2D image)
    {
        this.image = image;
        material.mainTexture = image;

        Vector2 logoSize = new Vector2(image.width, image.height);
        float maxDimension = Mathf.Max(logoSize.x, logoSize.y);
        float aspectWidth = logoSize.x / maxDimension;
        float aspectHeight = logoSize.y / maxDimension;

        material.SetFloat("_TexWidth", aspectWidth);
        material.SetFloat("_TexHeight", aspectHeight);

        PositionAtTopBorder();
    }

    void PositionAtTopBorder()
    {
        if (targetCamera == null) return;

        var viewportPos = new Vector3(0.5f, 1f - borderPadding, distanceFromCamera);
        var worldPos = targetCamera.ViewportToWorldPoint(viewportPos);
        transform.position = worldPos;

        transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position, Vector3.up);
    }

    public void SetActive(bool isActive)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(isActive);
        }
    }

}
