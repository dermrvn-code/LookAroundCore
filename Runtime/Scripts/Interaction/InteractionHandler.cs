using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionHandler : MonoBehaviour
{

    Hoverable target;
    EyesHandler eyesHandler;

    Dictionary<DomePosition, Hoverable> elements = new Dictionary<DomePosition, Hoverable>();

    [SerializeField]
    LayerMask layer;

    public bool updateElementsNextFrame = true;

    void Start()
    {
        eyesHandler = GetComponent<EyesHandler>();
    }

    void UpdateElements()
    {
        updateElementsNextFrame = false;
        elements = new Dictionary<DomePosition, Hoverable>();
        target = null;
        var domeElements = FindObjectsByType<DomePosition>(FindObjectsSortMode.None);
        foreach (var domeElement in domeElements)
        {
            Hoverable elementToAdd;
            if (domeElement.GetComponent<Hoverable>() != null)
            {
                elementToAdd = domeElement.GetComponent<Hoverable>();
            }
            else if (domeElement.GetComponentInChildren<Hoverable>() != null)
            {
                elementToAdd = domeElement.GetComponentInChildren<Hoverable>();
            }
            else
            {
                continue;
            }
            elements.Add(domeElement, elementToAdd);
        }
    }

    public void Interact()
    {
        if (target == null) return;
        Interactable interactableTarget;
        if (target.TryGetComponent(out interactableTarget))
        {
            interactableTarget.Interact();
        }
    }


    // Update is called once per frame
    float oldRotation = -20;
    bool checkedElements = false;
    int offset = 6;
    bool foundTarget = false;
    void Update()
    {
        if (updateElementsNextFrame) UpdateElements();

        if (eyesHandler.rotation != oldRotation)
        {
            if (!foundTarget)
            {
                target?.Unhighlight();
                target = null;
            }
            oldRotation = eyesHandler.rotation;
            checkedElements = false;
        }
        else if (!checkedElements)
        {
            foundTarget = false;
            checkedElements = true;
            if (elements.Count > 0)
            {
                foreach (var element in elements)
                {
                    float elementX = element.Key.position.x;
                    float min = (elementX - offset + 360) % 360;
                    float max = (elementX + offset) % 360;

                    bool isBetween;
                    if (min < max)
                    {
                        isBetween = oldRotation >= min && oldRotation <= max;
                    }
                    else
                    {
                        isBetween = oldRotation >= min || oldRotation <= max;
                    }

                    if (isBetween)
                    {
                        target = element.Value;
                        foundTarget = true;
                    }
                }
                target?.Highlight();
            }
        }
    }
}
