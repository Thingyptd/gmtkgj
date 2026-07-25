using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    public CharacterManager characterManager;

    [Header("Moves Counter")]
    public TextMeshProUGUI movesText;

    [Header("Floor Display")]
    public TextMeshProUGUI floorText;

    [Header("Queue Display")]
    public GameObject queueIconPrefab;
    public Transform queueContainer; 

    private GridMovement currentCharacter;
    private List<GameObject> queueIcons = new List<GameObject>();

    void OnEnable()
    {
        if (characterManager == null)
            characterManager = FindAnyObjectByType<CharacterManager>();

        characterManager.OnCharacterSpawned += HandleCharacterSpawned;
        characterManager.OnCharacterDied += HandleCharacterDied;
        characterManager.OnGameOver += HandleGameOver;

        RefreshFloorText();
    }

    void OnDisable()
    {
        if (characterManager == null) return;

        characterManager.OnCharacterSpawned -= HandleCharacterSpawned;
        characterManager.OnCharacterDied -= HandleCharacterDied;
        characterManager.OnGameOver -= HandleGameOver;

        if (currentCharacter != null)
            currentCharacter.OnMovesChanged -= HandleMovesChanged;
    }

    private void HandleCharacterSpawned(CharacterData data, GridMovement character)
    {
        if (currentCharacter != null)
            currentCharacter.OnMovesChanged -= HandleMovesChanged;

        currentCharacter = character;
        currentCharacter.OnMovesChanged += HandleMovesChanged;

        HandleMovesChanged(character.movesRemaining, character.data.moveRange);

        RefreshQueue();
    }

    private void HandleCharacterDied(CharacterData data)
    {
        if (currentCharacter != null)
        {
            currentCharacter.OnMovesChanged -= HandleMovesChanged;
            currentCharacter = null;
        }
    }

    private void HandleMovesChanged(int remaining, int max)
    {
        if (movesText != null)
            movesText.text = remaining.ToString();
    }

    private void RefreshQueue()
    {
        foreach (var icon in queueIcons)
            Destroy(icon);
        queueIcons.Clear();

        if (queueContainer == null || queueIconPrefab == null) return;

        var session = GameSession.Instance;
        if (session == null) return;

        for (int i = session.currentCharacterIndex; i < session.equippedCharacters.Count; i++)
        {
            CharacterData data = session.equippedCharacters[i];

            GameObject iconGO = Instantiate(queueIconPrefab, queueContainer);
            Image img = iconGO.GetComponent<Image>();
            if (img != null)
                img.color = data.characterColor;

            iconGO.transform.localScale = (i == session.currentCharacterIndex) ? Vector3.one * 1.3f : Vector3.one;

            queueIcons.Add(iconGO);
        }
    }

    private void HandleGameOver()
    {
        if (movesText != null)
            movesText.text = "GAME OVER";

        foreach (var icon in queueIcons)
            Destroy(icon);
        queueIcons.Clear();
    }

    private void RefreshFloorText()
    {
        if (floorText == null) return;

        var session = GameSession.Instance;
        if (session == null) return;

        floorText.text = $"F{session.CurrentFloorIndex + 1}";
    }
}