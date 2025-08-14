using System;
using UnityEngine;
using TMPro;

public abstract class ProgressBarBase : MonoBehaviour
{
    [SerializeField]
    protected TMP_Text progressText;

    protected string progressTextPrefix = "Loading: ";
    protected float progressValue = 0f;
    protected float currentProgressValue = 0f;
    protected float progressSpeed = 2f;
    protected int currentStep;
    protected int totalSteps;
    protected bool show = false;

    protected Action onFull;

    public virtual void Update()
    {
    }


    public void OnFull(Action action)
    {
        onFull = () =>
        {
            action?.Invoke();
            this.onFull = null;
            currentStep = 0;
        };
    }

    public void UpdateBarIncreaseSteps(int totalSteps, string message, int step = 1)
    {
        currentStep += step;
        this.totalSteps = totalSteps;
        if (currentStep > totalSteps) currentStep = totalSteps;

        UpdateBar(message);
    }

    public void UpdateBar(int step, int totalSteps, string message)
    {
        currentStep = step;
        this.totalSteps = totalSteps;
        UpdateBar(message);
    }

    public void UpdateBar(string message)
    {
        show = currentStep < totalSteps;
        progressValue = totalSteps > 0 ? (float)currentStep / totalSteps : 0f;

        if (!show)
        {
            onFull?.Invoke();
        }
        UpdateProgressText(message);
    }

    public void UpdateProgressText(string message)
    {
        if (progressText != null)
        {
            progressText.text = progressTextPrefix + message;
        }
    }
}
