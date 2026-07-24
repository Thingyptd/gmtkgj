using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Squadra (impostata in editor per il livello iniziale)")]
    public List<CharacterData> equippedCharacters = new List<CharacterData>();

    [Header("Scene dei piani, in ordine")]
    public List<string> floorSceneNames = new List<string>();

    // Stato runtime della squadra: indice del personaggio corrente e mosse residue
    public int currentCharacterIndex = 0;
    public int currentCharacterMovesRemaining = -1; // -1 = non ancora inizializzato per il personaggio corrente

    private int currentFloorSceneIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public CharacterData CurrentCharacterData =>
        currentCharacterIndex < equippedCharacters.Count ? equippedCharacters[currentCharacterIndex] : null;

    public bool HasCharactersLeft => currentCharacterIndex < equippedCharacters.Count;

    public void AdvanceToNextCharacter()
    {
        currentCharacterIndex++;
        currentCharacterMovesRemaining = -1; // il prossimo CharacterManager lo inizializzerà da CharacterData.moveRange
    }

    public void GoToNextFloor()
    {
        currentFloorSceneIndex++;

        if (currentFloorSceneIndex >= floorSceneNames.Count)
        {
            Debug.Log("Hai completato tutti i piani! Vittoria.");
            // qui in futuro: scena di vittoria
            return;
        }

        SceneManager.LoadScene(floorSceneNames[currentFloorSceneIndex]);
    }

    public void ResetCurrentFloor()
    {
        // Reset del piano: la scena si ricarica da zero (massi, trappole, layout tornano come in editor).
        // NOTA: la squadra equipaggiata NON viene toccata qui, resta quella attuale
        // (decidi tu se un game over debba anche resettare la squadra, vedi ResetEntireRun sotto)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetEntireRun()
    {
        // Reset completo: squadra ripristinata, si torna al primo piano
        currentCharacterIndex = 0;
        currentCharacterMovesRemaining = -1;
        currentFloorSceneIndex = 0;
        SceneManager.LoadScene(floorSceneNames[0]);
    }

    public int CurrentFloorIndex => currentFloorSceneIndex;
    public int TotalFloors => floorSceneNames.Count;
}