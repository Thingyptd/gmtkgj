using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Hover")]
    public float hoverScale = 1.05f;
    public float hoverDuration = 0.15f;

    [Header("Click Wiggle (rotazione sinistra/destra)")]
    public float wiggleAngle = 8f;
    public float wiggleDuration = 0.3f;
    public int wiggleVibrato = 3; 

    [Header("Disabled Feedback")]
    public float disabledWiggleAngle = 6f;
    public float disabledWiggleDuration = 0.25f;

    private Button button;
    private RectTransform rect;
    private Vector3 baseScale;
    private Action pendingAction;

    void Awake()
    {
        button = GetComponent<Button>();
        rect = GetComponent<RectTransform>();
        baseScale = rect.localScale;

        button.onClick.AddListener(HandleValidClick);
    }

    public void SetAction(Action action)
    {
        pendingAction = action;
    }

    private void HandleValidClick()
    {
        PlayWiggle(wiggleAngle, wiggleDuration, () => pendingAction?.Invoke());
    }

    private void PlayWiggle(float angle, float duration, Action onComplete = null)
    {
        rect.DOKill();
        rect.localRotation = Quaternion.identity;

        Sequence seq = DOTween.Sequence();
        float step = duration / (wiggleVibrato * 2f);

        for (int i = 0; i < wiggleVibrato; i++)
        {
            float currentAngle = angle * (1f - (float)i / wiggleVibrato); 
            seq.Append(rect.DOLocalRotate(new Vector3(0, 0, -currentAngle), step).SetEase(Ease.InOutSine));
            seq.Append(rect.DOLocalRotate(new Vector3(0, 0, currentAngle), step).SetEase(Ease.InOutSine));
        }

        seq.Append(rect.DOLocalRotate(Vector3.zero, step).SetEase(Ease.OutQuad));
        seq.AppendCallback(() => onComplete?.Invoke());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        rect.DOKill();
        rect.DOScale(baseScale * hoverScale, hoverDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rect.DOKill();
        rect.DOScale(baseScale, hoverDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!button.interactable)
        {
            PlayWiggle(disabledWiggleAngle, disabledWiggleDuration);
        }
    }
}