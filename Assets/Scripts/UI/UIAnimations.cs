using DG.Tweening;
using UnityEngine;

/// <summary>
/// Libreria statica di animazioni UI riutilizzabili. Nessuno stato qui dentro:
/// ogni chiamante gestisce i propri riferimenti (RectTransform, CanvasGroup, ecc.).
/// </summary>
public static class UIAnimations
{
    public static Tween PunchScale(Transform target, float punch = 0.15f, float duration = 0.2f)
    {
        target.DOKill();
        return target.DOPunchScale(Vector3.one * punch, duration, 6, 0.5f);
    }

    public static Tween ShakePosition(RectTransform target, float strength = 12f, float duration = 0.4f)
    {
        target.DOKill();
        return target.DOShakeAnchorPos(duration, strength, vibrato: 20, randomness: 90, snapping: false, fadeOut: true);
    }

    /// <summary>Slide + fade in da un offset relativo alla posizione "a riposo" del RectTransform.</summary>
    public static Sequence SlideAndFadeIn(RectTransform rt, CanvasGroup cg, Vector2 fromOffset, float duration = 0.4f, Ease ease = Ease.OutCubic)
    {
        Vector2 restPos = rt.anchoredPosition;
        rt.anchoredPosition = restPos + fromOffset;
        cg.alpha = 0f;
        cg.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();
        seq.Join(rt.DOAnchorPos(restPos, duration).SetEase(ease));
        seq.Join(cg.DOFade(1f, duration).SetEase(Ease.OutQuad));
        seq.AppendCallback(() => cg.blocksRaycasts = true); 
        return seq;
    }

    /// <summary>Slide + fade out verso un offset relativo alla posizione attuale.</summary>
    public static Sequence SlideAndFadeOut(RectTransform rt, CanvasGroup cg, Vector2 toOffset, float duration = 0.3f, Ease ease = Ease.InCubic)
    {
        cg.blocksRaycasts = false;
        Vector2 targetPos = rt.anchoredPosition + toOffset;

        Sequence seq = DOTween.Sequence();
        seq.Join(rt.DOAnchorPos(targetPos, duration).SetEase(ease));
        seq.Join(cg.DOFade(0f, duration).SetEase(Ease.InQuad));
        return seq;
    }

    /// <summary>Fa apparire in cascata i figli diretti di un container (scale 0 -> 1, con delay incrementale).</summary>
    public static void StaggerChildrenIn(Transform container, float delayPerChild = 0.06f, float duration = 0.3f)
    {
        int i = 0;
        foreach (Transform child in container)
        {
            child.localScale = Vector3.zero;
            child.DOScale(Vector3.one, duration)
                 .SetEase(Ease.OutBack)
                 .SetDelay(i * delayPerChild);
            i++;
        }
    }
}