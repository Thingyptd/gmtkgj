using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CharacterManager : MonoBehaviour
{
    public Grid grid;
    public Tilemap collisionTilemap;
    public Tilemap pitsTilemap;

    public GameObject characterPrefab;
    public Transform spawnPoint;

    public float deathDelay = 0.2f;

    public GameObject divineBlastPrefab;

    public GameObject deathParticlePrefab;
    public float deathParticleLifetime = 2f;

    public event Action<CharacterData, GridMovement> OnCharacterSpawned;
    public event Action<CharacterData> OnCharacterDied;
    public event Action<CharacterData> OnCharacterFullyDied;
    public event Action OnGameOver;

    public GridMovement CurrentCharacter => currentCharacter;

    private GridMovement currentCharacter;
    private Vector3 lastPosition;
    private Vector3 lastGroundPosition;
    private bool lastFacingLeft = false;

    void Awake()
    {
        if (grid == null) grid = FindAnyObjectByType<Grid>();
        ScreenTransition.Instance.Open();

        OnGameOver += HandleGameOverReset;
    }

    private void HandleGameOverReset()
    {
        if (ScreenTransition.Instance != null)
        {
            ScreenTransition.Instance.Close(() =>
            {
                GameSession.Instance.ResetCurrentFloor();
            });
        }
        else
        {
            GameSession.Instance.ResetCurrentFloor();
        }
    }

    public void BeginFloor()
    {
        lastPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        lastGroundPosition = lastPosition;
        SpawnCurrentCharacter();
    }

    private void SpawnCurrentCharacter()
    {
        var session = GameSession.Instance;
        if (session == null || !session.HasCharactersLeft)
        {
            GameOver();
            return;
        }

        CharacterData data = session.CurrentCharacterData;

        GameObject instance = Instantiate(characterPrefab);
        currentCharacter = instance.GetComponent<GridMovement>();

        currentCharacter.grid = grid;
        currentCharacter.OnMovesExhausted += HandleMovesExhausted;
        currentCharacter.OnFellIntoPit += HandleFellIntoPit;
        currentCharacter.OnGroundCellEntered += HandleGroundTouched;
        currentCharacter.OnStairsEntered += HandleStairsEntered;

        currentCharacter.Initialize(data, lastPosition, collisionTilemap, pitsTilemap, lastFacingLeft);

        if (session.currentCharacterMovesRemaining >= 0)
        {
            currentCharacter.movesRemaining = session.currentCharacterMovesRemaining;
        }

        OnCharacterSpawned?.Invoke(data, currentCharacter);
        Debug.Log($"Spawnato: {data.characterName} (mosse: {currentCharacter.movesRemaining})");
    }

    private void HandleGroundTouched(Vector3 worldPos) => lastGroundPosition = worldPos;

    private void HandleFellIntoPit(GridMovement character)
    {
        character.RecoverFromFall(lastGroundPosition);
    }

    private void HandleStairsEntered(GridMovement character)
    {
        ScreenTransition.Instance.Close(() =>
        {
            GameSession.Instance.GoToNextFloor();
            FMODEvents.Instance.PlayStartSound();
        });
    }

    private void HandleMovesExhausted(GridMovement deadCharacter)
    {
        CharacterData deadData = GameSession.Instance.CurrentCharacterData;
        Debug.Log($"{deadData.characterName} ha esaurito le mosse.");

        lastFacingLeft = deadCharacter.IsFacingLeft;
        lastPosition = deadCharacter.GetTargetWorldPosition();

        deadCharacter.OnMovesExhausted -= HandleMovesExhausted;
        deadCharacter.OnFellIntoPit -= HandleFellIntoPit;
        deadCharacter.OnGroundCellEntered -= HandleGroundTouched;
        deadCharacter.OnStairsEntered -= HandleStairsEntered;

        OnCharacterDied?.Invoke(deadData);

        StartCoroutine(DeathSequence(deadCharacter, deadData));
    }

    private IEnumerator DeathSequence(GridMovement deadCharacter, CharacterData deadData)
    {
        if (deathDelay > 0f)
            yield return new WaitForSeconds(deathDelay);

        Vector3 blastPosition = deadCharacter != null ? deadCharacter.GetTargetWorldPosition() : lastPosition;

        GameObject blastInstance = null;
        float blastDuration = 0f;

        if (divineBlastPrefab != null)
        {
            blastInstance = Instantiate(divineBlastPrefab, blastPosition, Quaternion.identity);

            DivineBlastEffect blast = blastInstance.GetComponent<DivineBlastEffect>();

            if (blast != null)
            {
                blast.Play();
                blastDuration = blast.AnimationDuration;
            }
            else
            {
                Debug.LogError($"Il prefab '{divineBlastPrefab.name}' non ha il componente DivineBlastEffect sul GameObject radice.");
            }
        }

        float halfDuration = blastDuration * 0.5f;
        if (halfDuration > 0f)
            yield return new WaitForSeconds(halfDuration);

        if (deadCharacter != null)
        {
            if (deathParticlePrefab != null)
            {
                GameObject particleInstance = Instantiate(deathParticlePrefab, blastPosition, Quaternion.identity);

                if (deathParticleLifetime > 0f)
                    Destroy(particleInstance, deathParticleLifetime);
            }

            Destroy(deadCharacter.gameObject);
        }

        currentCharacter = null;

        float remainingDuration = blastDuration - halfDuration;
        if (remainingDuration > 0f)
            yield return new WaitForSeconds(remainingDuration);

        if (blastInstance != null)
            Destroy(blastInstance);

        OnCharacterFullyDied?.Invoke(deadData);

        GameSession.Instance.AdvanceToNextCharacter();
        SpawnCurrentCharacter();
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER: tutti i personaggi equipaggiati sono stati esauriti.");
        OnGameOver?.Invoke();
    }
}