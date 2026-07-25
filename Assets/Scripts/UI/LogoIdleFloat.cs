using DG.Tweening;
using UnityEngine;

public class LogoIdleFloat : MonoBehaviour
{
    public float floatDistance = 10f;
    public float floatDuration = 2f;

    void Start()
    {
        transform.DOLocalMoveY(transform.localPosition.y + floatDistance, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}