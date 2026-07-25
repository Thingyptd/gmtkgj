using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenTransition : MonoBehaviour
{
    [Header("References")]
    public Image wipeImage;

    [Header("Band thickness (in pixel, calcolato in base alla dimensione reale del rettangolo)")]
    public float thicknessPixels = 60f;

    [Header("Timing")]
    public float closeDuration = 1.2f;
    public float openDuration = 1.2f;
    public AnimationCurve easing = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Band Thickness")]
    public float thicknessPixelsX = 60f;
    public float thicknessPixelsY = 60f;

    private Material material;
    private static readonly int ProgressID = Shader.PropertyToID("_Progress");
    private static readonly int ThicknessXID = Shader.PropertyToID("_ThicknessX");
    private static readonly int ThicknessYID = Shader.PropertyToID("_ThicknessY");
    private static readonly int MaxStepsID = Shader.PropertyToID("_MaxSteps");

    public static ScreenTransition Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        material = Instantiate(wipeImage.material);
        wipeImage.material = material;

        DontDestroyOnLoad(transform.root.gameObject);

        ApplyThickness();
        SetProgress(0f);
    }

    private void ApplyThickness()
    {
        RectTransform rt = wipeImage.rectTransform;
        float width = rt.rect.width;
        float height = rt.rect.height;

        float tx = thicknessPixelsX / width;
        float ty = thicknessPixelsY / height;

        material.SetFloat(ThicknessXID, tx);
        material.SetFloat(ThicknessYID, ty);

        float maxLayer = 0.5f / Mathf.Min(tx, ty);
        float maxSteps = Mathf.Ceil(maxLayer) + 1f;
        material.SetFloat(MaxStepsID, maxSteps);
    }

    private void SetProgress(float p) => material.SetFloat(ProgressID, p);

    public void Close(Action onComplete = null)
    {
        ApplyThickness(); 
        StartCoroutine(Animate(0f, 1f, closeDuration, onComplete));
    }

    public void Open(Action onComplete = null)
    {
        StartCoroutine(Animate(1f, 0f, openDuration, onComplete));
    }

    private IEnumerator Animate(float from, float to, float duration, Action onComplete)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = easing.Evaluate(Mathf.Clamp01(t / duration));
            SetProgress(Mathf.Lerp(from, to, p));
            yield return null;
        }
        SetProgress(to);
        onComplete?.Invoke();
    }
}