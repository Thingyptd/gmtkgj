using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EyeIndicatorUI : MonoBehaviour
{
    public CharacterManager characterManager;
    public Image eyeImage;
    public RectTransform hoverRoot;

    public Sprite[] frames;

    public float openTweenDuration = 0.25f;
    public float closeOnDeathDuration = 0.5f;

    public float hoverDistance = 6f;
    public float hoverDuration = 1.4f;

    public int shakeFramesFromEnd = 2;
    public float shakeStrength = 6f;
    public float shakeDuration = 0.3f;

    private CanvasGroup canvasGroup;
    private float currentOpenness = 0f;
    private Tween openTween;
    private GridMovement subscribedCharacter;
    private bool shakeTriggeredForThisCharacter = false;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    void Start()
    {
        if (characterManager == null)
            characterManager = FindAnyObjectByType<CharacterManager>();

        characterManager.OnCharacterSpawned += HandleCharacterSpawned;
        characterManager.OnCharacterFullyDied += HandleCharacterFullyDied;
        characterManager.OnGameOver += HandleGameOver;

        SetOpenness(0f, instant: true);
        StartHoverLoop();

        if (characterManager.CurrentCharacter != null)
        {
            GridMovement existing = characterManager.CurrentCharacter;
            HandleCharacterSpawned(existing.data, existing);
        }
    }

    void OnDestroy()
    {
        if (characterManager != null)
        {
            characterManager.OnCharacterSpawned -= HandleCharacterSpawned;
            characterManager.OnCharacterFullyDied -= HandleCharacterFullyDied;
            characterManager.OnGameOver -= HandleGameOver;
        }

        UnsubscribeCurrent();
    }

    public void SetVisible(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
    }

    private void StartHoverLoop()
    {
        float baseY = hoverRoot.anchoredPosition.y;

        hoverRoot.DOAnchorPosY(baseY + hoverDistance, hoverDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void HandleCharacterSpawned(CharacterData data, GridMovement character)
    {
        SetVisible(true);

        UnsubscribeCurrent();

        subscribedCharacter = character;
        subscribedCharacter.OnMovesChanged += HandleMovesChanged;

        shakeTriggeredForThisCharacter = false;

        HandleMovesChanged(character.movesRemaining, data.moveRange);
    }

    private void UnsubscribeCurrent()
    {
        if (subscribedCharacter == null) return;

        subscribedCharacter.OnMovesChanged -= HandleMovesChanged;
        subscribedCharacter = null;
    }

    private void HandleMovesChanged(int remaining, int max)
    {
        float progress = max > 0 ? 1f - (float)remaining / max : 1f;
        progress = Mathf.Clamp01(progress);

        SetOpenness(progress, instant: false);

        int targetFrame = ProgressToFrameIndex(progress);
        bool nearMax = targetFrame >= frames.Length - shakeFramesFromEnd;

        if (nearMax && !shakeTriggeredForThisCharacter)
        {
            shakeTriggeredForThisCharacter = true;
            PlayShake();
        }
    }

    private void HandleCharacterFullyDied(CharacterData data)
    {
        openTween?.Kill();
        openTween = DOTween.To(() => currentOpenness, x =>
        {
            currentOpenness = x;
            UpdateSpriteFromOpenness();
        }, 0f, closeOnDeathDuration).SetEase(Ease.InQuad);
    }

    private void HandleGameOver()
    {
        SetVisible(false);
    }

    private void SetOpenness(float target, bool instant)
    {
        openTween?.Kill();

        if (instant)
        {
            currentOpenness = target;
            UpdateSpriteFromOpenness();
            return;
        }

        openTween = DOTween.To(() => currentOpenness, x =>
        {
            currentOpenness = x;
            UpdateSpriteFromOpenness();
        }, target, openTweenDuration).SetEase(Ease.OutQuad);
    }

    private void UpdateSpriteFromOpenness()
    {
        if (frames == null || frames.Length == 0) return;
        int index = ProgressToFrameIndex(currentOpenness);
        eyeImage.sprite = frames[index];
    }

    private int ProgressToFrameIndex(float progress)
    {
        if (frames == null || frames.Length == 0) return 0;
        return Mathf.RoundToInt(progress * (frames.Length - 1));
    }

    private void PlayShake()
    {
        eyeImage.rectTransform.DOShakeAnchorPos(shakeDuration, shakeStrength, vibrato: 20, randomness: 90, fadeOut: true);
    }
}