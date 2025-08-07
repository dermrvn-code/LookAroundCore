using System;
using UnityEngine;
using TMPro;

public abstract class ProgressBar : MonoBehaviour
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

    void Update()
    {
        _Update();
    }

    public abstract void _Update();

    public void OnFull(Action action)
    {
        onFull = () =>
        {
            action?.Invoke();
            onFull = null;
        };
    }

    public void UpdateBarIncreaseSteps(int step, int totalSteps, string message)
    {
        currentStep += step;
        this.totalSteps = totalSteps;
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

    void UpdateProgressText(string message)
    {
        if (progressText != null)
        {
            progressText.text = progressTextPrefix + message;
        }
    }
}
