using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EyeIndicatorUI : MonoBehaviour
{
    [Header("References")]
    public CharacterManager characterManager;
    public Image eyeImage;
    [Tooltip("RectTransform GENITORE di eyeImage: qui gira l'hover continuo, separato dallo shake per evitare conflitti DOTween sulla stessa proprietà")]
    public RectTransform hoverRoot;

    [Header("Frames (0 = completamente chiuso, ultimo = completamente aperto)")]
    public Sprite[] frames;

    [Header("Open/Close Tween")]
    [Tooltip("Durata del tween quando l'apertura cambia in base alle mosse rimanenti")]
    public float openTweenDuration = 0.25f;
    [Tooltip("Durata dell'animazione di chiusura quando il personaggio muore")]
    public float closeOnDeathDuration = 0.5f;

    [Header("Idle Hover")]
    public float hoverDistance = 6f;
    public float hoverDuration = 1.4f;

    [Header("Shake vicino all'apertura massima")]
    [Tooltip("Quanti frame dalla fine (compreso l'ultimo) contano come 'quasi al massimo'")]
    public int shakeFramesFromEnd = 2;
    public float shakeStrength = 6f;
    public float shakeDuration = 0.3f;

    private float currentOpenness = 0f; // 0 = chiuso, 1 = aperto
    private Tween openTween;
    private GridMovement subscribedCharacter;
    private bool shakeTriggeredForThisCharacter = false;

    void Start()
    {
        if (characterManager == null)
            characterManager = FindAnyObjectByType<CharacterManager>();

        characterManager.OnCharacterSpawned += HandleCharacterSpawned;

        SetOpenness(0f, instant: true);
        StartHoverLoop();
    }

    void OnDestroy()
    {
        if (characterManager != null)
            characterManager.OnCharacterSpawned -= HandleCharacterSpawned;

        UnsubscribeCurrent();
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
        UnsubscribeCurrent();

        subscribedCharacter = character;
        subscribedCharacter.OnMovesChanged += HandleMovesChanged;
        subscribedCharacter.OnMovesExhausted += HandleCharacterDied;

        shakeTriggeredForThisCharacter = false;

        // Lo stato iniziale va letto subito: OnMovesChanged è già scattato dentro Initialize()
        // PRIMA che potessimo iscriverci a questo evento (stesso motivo per cui il contatore
        // mosse in UIManager fa la stessa lettura immediata dopo l'iscrizione).
        HandleMovesChanged(character.movesRemaining, data.moveRange);
    }

    private void UnsubscribeCurrent()
    {
        if (subscribedCharacter == null) return;

        subscribedCharacter.OnMovesChanged -= HandleMovesChanged;
        subscribedCharacter.OnMovesExhausted -= HandleCharacterDied;
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

    private void HandleCharacterDied(GridMovement character)
    {
        // Animazione di chiusura alla morte, indipendentemente da quanto era aperto in quel momento
        openTween?.Kill();
        openTween = DOTween.To(() => currentOpenness, x =>
        {
            currentOpenness = x;
            UpdateSpriteFromOpenness();
        }, 0f, closeOnDeathDuration).SetEase(Ease.InQuad);
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
        // Shake sul figlio (eyeImage), MAI su hoverRoot: altrimenti competerebbe con
        // il tween di hover continuo sulla stessa proprietà anchoredPosition.
        eyeImage.rectTransform.DOShakeAnchorPos(shakeDuration, shakeStrength, vibrato: 20, randomness: 90, fadeOut: true);
    }
}