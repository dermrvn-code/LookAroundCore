using UnityEngine;

public abstract class ViewHandler : MonoBehaviour
{
    [Header("View Settings")]
    public float rotation = 0f;
    public float heightOffset = 0f;
    public int zoom = 0;

    [SerializeField] protected int minZoom = 25;
    [SerializeField] protected int maxZoom = 100;

    public float rotationSpeed = 2f;
    public float zoomSpeed = 2f;

    protected float currentRotation;
    protected float rotationVelocity = 0.0f;

    protected float currentZoom;
    protected float zoomVelocity = 0.0f;

    public virtual void Update()
    {
        UpdateRotation();
        UpdateZoom();
    }


    public abstract void UpdateRotation();
    public abstract void SetRotation(float rot);

    public abstract void UpdateZoom();
    public abstract void SetZoom(float newZoom);

    protected float Map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    public void ZoomIn()
    {
        if (zoom <= minZoom) return;
        zoom--;
        SetZoom(zoom);
    }

    public void ZoomOut()
    {
        if (zoom >= maxZoom) return;
        zoom++;
        SetZoom(zoom);
    }

    public void LeftMove()
    {
        SetRotation(rotation - 1);
    }

    public void RightMove()
    {
        SetRotation(rotation + 1);
    }
}
