using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PanelTransition : MonoBehaviour
{
    public enum SlideDirection { Left, Right, Up, Down }

    [Header("Config")]
    public SlideDirection direction = SlideDirection.Right;
    public float slideDistance = 500f;
    public float duration = 0.4f;
    public bool staggerChildren = false;

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Vector2 restPosition; // posizione "di riposo" originale, catturata UNA SOLA VOLTA

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        restPosition = rect.anchoredPosition; // catturata qui, PRIMA che qualunque animazione la sposti
    }

    private Vector2 GetOffset()
    {
        switch (direction)
        {
            case SlideDirection.Left: return new Vector2(-slideDistance, 0);
            case SlideDirection.Right: return new Vector2(slideDistance, 0);
            case SlideDirection.Up: return new Vector2(0, slideDistance);
            default: return new Vector2(0, -slideDistance);
        }
    }

    public void Show(Action onComplete = null)
    {
        gameObject.SetActive(true);

        // Parte SEMPRE da restPosition + offset, non da dove si trovava prima
        rect.anchoredPosition = restPosition + GetOffset();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();
        seq.Join(rect.DOAnchorPos(restPosition, duration).SetEase(Ease.OutCubic));
        seq.Join(canvasGroup.DOFade(1f, duration).SetEase(Ease.OutQuad));
        seq.AppendCallback(() => canvasGroup.blocksRaycasts = true);

        if (staggerChildren)
            seq.AppendCallback(() => UIAnimations.StaggerChildrenIn(transform, 0.06f, 0.3f));

        seq.AppendCallback(() => onComplete?.Invoke());
    }

    public void Hide(Action onComplete = null)
    {
        canvasGroup.blocksRaycasts = false;

        // Va SEMPRE verso restPosition + offset, calcolato dalla posizione di riposo nota,
        // non dalla posizione attuale (che dovrebbe già essere restPosition, ma non fidiamoci)
        Vector2 targetPos = restPosition + GetOffset();

        Sequence seq = DOTween.Sequence();
        seq.Join(rect.DOAnchorPos(targetPos, duration * 0.75f).SetEase(Ease.InCubic));
        seq.Join(canvasGroup.DOFade(0f, duration * 0.75f).SetEase(Ease.InQuad));
        seq.AppendCallback(() =>
        {
            gameObject.SetActive(false);
            rect.anchoredPosition = restPosition; // reset esplicito: la prossima Show() parte pulita
            onComplete?.Invoke();
        });
    }
}