using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SelectionItemAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Color States")]
    public Color idleColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    public Color baseColor = Color.white;
    public float colorTweenDuration = 0.15f;

    [Header("Hover Scale")]
    public float hoverScale = 1.08f;
    public float hoverScaleDuration = 0.15f;

    [Header("Click Feedback")]
    public float punchScale = 0.18f;
    public float punchDuration = 0.25f;

    private Button button;
    private RectTransform rect;
    private Image image;
    private Vector3 baseScale;
    private Action pendingAction;
    private bool isLocked = false;

    void Awake()
    {
        button = GetComponent<Button>();
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        baseScale = rect.localScale;

        button.transition = Selectable.Transition.None;

        if (image != null)
            image.color = idleColor;

        button.onClick.AddListener(HandleValidClick);
    }

    public void SetAction(Action action)
    {
        pendingAction = action;
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;

        if (image == null) return;

        image.DOKill();
        image.DOColor(locked ? baseColor : idleColor, colorTweenDuration);

        rect.DOKill();
        rect.DOScale(baseScale, hoverScaleDuration);
    }

    private void HandleValidClick()
    {
        pendingAction?.Invoke();

        rect.DOKill();
        rect.DOPunchScale(Vector3.one * punchScale, punchDuration, 6, 0.7f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable || isLocked) return;

        if (image != null)
        {
            image.DOKill();
            image.DOColor(baseColor, colorTweenDuration);
        }

        rect.DOKill();
        FMODEvents.Instance.PlayHoverSound();
        rect.DOScale(baseScale * hoverScale, hoverScaleDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isLocked) return;

        if (image != null)
        {
            image.DOKill();
            image.DOColor(idleColor, colorTweenDuration);
        }

        rect.DOKill();
        rect.DOScale(baseScale, hoverScaleDuration).SetEase(Ease.OutQuad);
    }
}