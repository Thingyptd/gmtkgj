using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CharacterManager : MonoBehaviour
{
    [Header("Grid & Tilemaps")]
    public Grid grid;
    public Tilemap collisionTilemap;
    public Tilemap pitsTilemap;

    [Header("Setup")]
    public GameObject characterPrefab;
    public Transform spawnPoint; 

    [Header("Timing")]
    public float deathDelay = 0.6f;

    public event Action<CharacterData, GridMovement> OnCharacterSpawned;
    public event Action<CharacterData> OnCharacterDied;
    public event Action OnGameOver;

    private GridMovement currentCharacter;
    private Vector3 lastPosition;
    private Vector3 lastGroundPosition;

    void Awake()
    {
        if (grid == null) grid = FindAnyObjectByType<Grid>();

        ScreenTransition.Instance.Open();
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

        currentCharacter.Initialize(data, lastPosition, collisionTilemap, pitsTilemap);

        // Se il personaggio arriva da un piano precedente con mosse già parzialmente consumate,
        // sovrascrivi movesRemaining con quello salvato in sessione
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
        });
    }

    private void HandleMovesExhausted(GridMovement deadCharacter)
    {
        CharacterData deadData = GameSession.Instance.CurrentCharacterData;
        Debug.Log($"{deadData.characterName} ha esaurito le mosse.");

        lastPosition = deadCharacter.GetCurrentWorldPosition();

        deadCharacter.OnMovesExhausted -= HandleMovesExhausted;
        deadCharacter.OnFellIntoPit -= HandleFellIntoPit;
        deadCharacter.OnGroundCellEntered -= HandleGroundTouched;
        deadCharacter.OnStairsEntered -= HandleStairsEntered;

        OnCharacterDied?.Invoke(deadData);

        StartCoroutine(DeathSequence(deadCharacter));
    }

    private IEnumerator DeathSequence(GridMovement deadCharacter)
    {
        yield return new WaitForSeconds(deathDelay);

        if (deadCharacter != null)
            Destroy(deadCharacter.gameObject);

        GameSession.Instance.AdvanceToNextCharacter();
        SpawnCurrentCharacter();
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER: tutti i personaggi equipaggiati sono stati esauriti.");
        OnGameOver?.Invoke();
    }
}