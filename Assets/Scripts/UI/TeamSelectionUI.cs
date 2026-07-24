using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectionUI : MonoBehaviour
{
    [Header("References")]
    public CharacterManager characterManager;

    public GameObject teamSelectionPanelRoot;

    [Tooltip("Tutti i personaggi assegnabili su questo piano")]
    public List<CharacterData> availableRoster = new List<CharacterData>();

    [Header("Contenitore che si nasconde con 'Vedi livello'")]
    public GameObject selectionContentRoot;

    [Header("Pannello Disponibili")]
    public Transform availableContainer;
    public GameObject availableItemPrefab; // figli attesi: NameText, ColorImage, e il Button è sull'oggetto radice

    [Header("Pannello Squadra")]
    public Transform teamContainer;
    public GameObject teamItemPrefab; // figli attesi: NameText, ColorImage (nessun Button necessario)

    [Header("Bottoni principali")]
    public Button undoButton;
    public Button confirmButton;
    public Button previewLevelButton;

    [Header("Popup di conferma")]
    public GameObject confirmDialogRoot;
    public Button confirmDialogYesButton;
    public Button confirmDialogNoButton;

    private List<CharacterData> selectedOrder = new List<CharacterData>();
    private List<Button> availableButtons = new List<Button>();
    private List<Image> availableImages = new List<Image>();
    private List<GameObject> teamRowInstances = new List<GameObject>();

    private bool contentVisible = true;

    void Start()
    {
        BuildAvailablePanel();

        undoButton.onClick.AddListener(OnUndoClicked);
        confirmButton.onClick.AddListener(OnConfirmClicked);
        previewLevelButton.onClick.AddListener(OnPreviewToggle);
        confirmDialogYesButton.onClick.AddListener(OnConfirmDialogYes);
        confirmDialogNoButton.onClick.AddListener(OnConfirmDialogNo);

        confirmDialogRoot.SetActive(false);
        RefreshUI();
    }

    private void BuildAvailablePanel()
    {
        for (int i = 0; i < availableRoster.Count; i++)
        {
            CharacterData data = availableRoster[i];

            GameObject itemGO = Instantiate(availableItemPrefab, availableContainer);

            var nameText = itemGO.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null) nameText.text = data.characterName;

            var colorImage = itemGO.transform.Find("ColorImage")?.GetComponent<Image>();
            if (colorImage != null) colorImage.color = data.characterColor;

            Button button = itemGO.GetComponent<Button>();
            int index = i; // cattura locale per la closure
            button.onClick.AddListener(() => OnAvailableClicked(index));

            availableButtons.Add(button);
            availableImages.Add(colorImage);
        }
    }

    private void OnAvailableClicked(int index)
    {
        if (!availableButtons[index].interactable) return; // già selezionato

        CharacterData data = availableRoster[index];
        selectedOrder.Add(data);

        SetAvailableItemSelected(index, true);
        AddTeamRow(data);
        RefreshUI();
    }

    private void OnUndoClicked()
    {
        if (selectedOrder.Count == 0) return;

        CharacterData last = selectedOrder[selectedOrder.Count - 1];
        selectedOrder.RemoveAt(selectedOrder.Count - 1);

        int index = availableRoster.IndexOf(last);
        if (index >= 0) SetAvailableItemSelected(index, false);

        RemoveLastTeamRow();
        RefreshUI();
    }

    private void SetAvailableItemSelected(int index, bool selected)
    {
        availableButtons[index].interactable = !selected;

        if (availableImages[index] != null)
            availableImages[index].color = selected ? Color.gray : availableRoster[index].characterColor;
    }

    private void AddTeamRow(CharacterData data)
    {
        GameObject rowGO = Instantiate(teamItemPrefab, teamContainer);

        var nameText = rowGO.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null) nameText.text = data.characterName;

        var colorImage = rowGO.transform.Find("ColorImage")?.GetComponent<Image>();
        if (colorImage != null) colorImage.color = data.characterColor;

        teamRowInstances.Add(rowGO);
    }

    private void RemoveLastTeamRow()
    {
        if (teamRowInstances.Count == 0) return;

        int lastIndex = teamRowInstances.Count - 1;
        Destroy(teamRowInstances[lastIndex]);
        teamRowInstances.RemoveAt(lastIndex);
    }

    private void RefreshUI()
    {
        undoButton.interactable = selectedOrder.Count > 0;

        bool allSelected = selectedOrder.Count == availableRoster.Count && availableRoster.Count > 0;
        confirmButton.gameObject.SetActive(allSelected);
    }

    private void OnConfirmClicked()
    {
        confirmDialogRoot.SetActive(true);
    }

    private void OnConfirmDialogYes()
    {
        confirmDialogRoot.SetActive(false);

        GameSession.Instance.equippedCharacters = new List<CharacterData>(selectedOrder);
        GameSession.Instance.currentCharacterIndex = 0;
        GameSession.Instance.currentCharacterMovesRemaining = -1;

        teamSelectionPanelRoot.SetActive(false);
        characterManager.BeginFloor();
    }

    private void OnConfirmDialogNo()
    {
        confirmDialogRoot.SetActive(false);
    }

    private void OnPreviewToggle()
    {
        contentVisible = !contentVisible;
        selectionContentRoot.SetActive(contentVisible);
    }
}