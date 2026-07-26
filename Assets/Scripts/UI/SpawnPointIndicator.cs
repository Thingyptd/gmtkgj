using DG.Tweening;
using UnityEngine;

public class SpawnPointIndicator : MonoBehaviour
{
    public CharacterManager characterManager;
    public Transform spawnPoint;

    public float hoverDistance = 0.15f;
    public float hoverDuration = 0.8f;

    void Start()
    {
        if (characterManager == null)
            characterManager = FindAnyObjectByType<CharacterManager>();

        if (spawnPoint != null)
            transform.position = spawnPoint.position;

        characterManager.OnCharacterSpawned += HandleCharacterSpawned;

        StartHoverLoop();
    }

    void OnDestroy()
    {
        if (characterManager != null)
            characterManager.OnCharacterSpawned -= HandleCharacterSpawned;
    }

    private void StartHoverLoop()
    {
        float baseY = transform.position.y;

        transform.DOMoveY(baseY + hoverDistance, hoverDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void HandleCharacterSpawned(CharacterData data, GridMovement character)
    {
        characterManager.OnCharacterSpawned -= HandleCharacterSpawned;

        transform.DOKill();
        Destroy(gameObject);
    }
}