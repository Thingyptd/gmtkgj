using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

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

    public static Sequence SlideAndFadeOut(RectTransform rt, CanvasGroup cg, Vector2 toOffset, float duration = 0.3f, Ease ease = Ease.InCubic)
    {
        cg.blocksRaycasts = false;
        Vector2 targetPos = rt.anchoredPosition + toOffset;

        Sequence seq = DOTween.Sequence();
        seq.Join(rt.DOAnchorPos(targetPos, duration).SetEase(ease));
        seq.Join(cg.DOFade(0f, duration).SetEase(Ease.InQuad));
        return seq;
    }

    public static void StaggerChildrenIn(Transform container, float delayPerChild = 0.06f, float duration = 0.3f)
    {
        int i = 0;
        foreach (Transform child in container)
        {
            Vector3 originalScale = child.localScale;
            if (originalScale == Vector3.zero)
                originalScale = Vector3.one;

            child.localScale = Vector3.zero;
            child.DOScale(originalScale, duration)
                 .SetEase(Ease.OutBack)
                 .SetDelay(i * delayPerChild);
            i++;
        }
    }

    public static void FadeChildrenIn(Transform container, float delayPerChild = 0.06f, float duration = 0.3f)
    {
        int i = 0;
        foreach (Transform child in container)
        {
            Graphic graphic = child.GetComponent<Graphic>();
            if (graphic == null)
            {
                i++;
                continue;
            }

            Color c = graphic.color;
            c.a = 0f;
            graphic.color = c;

            graphic.DOFade(1f, duration).SetDelay(i * delayPerChild);

            i++;
        }
    }
}