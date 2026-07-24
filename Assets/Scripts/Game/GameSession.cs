using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Floor scenes")]
    public List<string> floorSceneNames = new List<string>();

    public List<CharacterData> equippedCharacters = new List<CharacterData>();
    public int currentCharacterIndex = 0;
    public int currentCharacterMovesRemaining = -1;

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
        currentCharacterMovesRemaining = -1;
    }

    public void GoToNextFloor()
    {
        currentFloorSceneIndex++;

        if (currentFloorSceneIndex >= floorSceneNames.Count)
        {
            Debug.Log("Tutti i piani completati");
            return;
        }

        SceneManager.LoadScene(floorSceneNames[currentFloorSceneIndex]);
    }

    public void ResetCurrentFloor()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetEntireRun()
    {
        currentCharacterIndex = 0;
        currentCharacterMovesRemaining = -1;
        currentFloorSceneIndex = 0;
        SceneManager.LoadScene(floorSceneNames[0]);
    }

    public int CurrentFloorIndex => currentFloorSceneIndex;
    public int TotalFloors => floorSceneNames.Count;
}