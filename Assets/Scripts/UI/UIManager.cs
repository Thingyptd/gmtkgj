using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public CharacterManager characterManager;

    public GameObject movesUIRoot;
    public TextMeshProUGUI movesText;
    public Image movesFillBar;

    public TextMeshProUGUI floorText;

    public GameObject queueIconPrefab;
    public Transform queueContainer;

    private GridMovement currentCharacter;
    private List<Image> queueIconImages = new List<Image>();

    void OnEnable()
    {
        if (characterManager == null)
            characterManager = FindAnyObjectByType<CharacterManager>();

        characterManager.OnCharacterSpawned += HandleCharacterSpawned;
        characterManager.OnCharacterDied += HandleCharacterDied;
        characterManager.OnGameOver += HandleGameOver;

        if (movesUIRoot != null)
            movesUIRoot.SetActive(false);

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
        if (movesUIRoot != null)
            movesUIRoot.SetActive(true);

        if (currentCharacter != null)
            currentCharacter.OnMovesChanged -= HandleMovesChanged;

        currentCharacter = character;
        currentCharacter.OnMovesChanged += HandleMovesChanged;

        HandleMovesChanged(character.movesRemaining, data.moveRange);

        if (queueIconImages.Count == 0)
            BuildQueueOnce();

        RefreshDeadStates();
    }

    private void HandleCharacterDied(CharacterData data)
    {
        if (currentCharacter != null)
        {
            currentCharacter.OnMovesChanged -= HandleMovesChanged;
            currentCharacter = null;
        }

        RefreshDeadStates();
    }

    private void HandleMovesChanged(int remaining, int max)
    {
        if (movesText != null)
            movesText.text = remaining.ToString();

        if (movesFillBar != null)
            movesFillBar.fillAmount = max > 0 ? (float)remaining / max : 0f;
    }

    private void BuildQueueOnce()
    {
        foreach (var icon in queueIconImages)
            Destroy(icon.gameObject);
        queueIconImages.Clear();

        if (queueContainer == null || queueIconPrefab == null) return;

        var session = GameSession.Instance;
        if (session == null) return;

        foreach (CharacterData data in session.equippedCharacters)
        {
            GameObject iconGO = Instantiate(queueIconPrefab, queueContainer);

            Image img = iconGO.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = data.idleFrame1;
                img.color = Color.white;
            }

            queueIconImages.Add(img);
        }
    }

    private void RefreshDeadStates()
    {
        var session = GameSession.Instance;
        if (session == null) return;

        for (int i = 0; i < queueIconImages.Count; i++)
        {
            if (queueIconImages[i] == null) continue;

            bool isDead = i < session.currentCharacterIndex;
            queueIconImages[i].color = isDead ? Color.gray : Color.white;
        }
    }

    private void RefreshFloorText()
    {
        if (floorText == null) return;

        var session = GameSession.Instance;
        if (session == null) return;

        floorText.text = $"Piano {session.CurrentFloorIndex + 1}/{session.TotalFloors}";
    }

    private void HandleGameOver()
    {
        if (movesUIRoot != null)
            movesUIRoot.SetActive(false);

        if (movesText != null)
            movesText.text = "";

        foreach (var icon in queueIconImages)
            Destroy(icon.gameObject);
        queueIconImages.Clear();
    }
}