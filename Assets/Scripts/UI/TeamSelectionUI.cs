using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectionUI : MonoBehaviour
{
    [Header("References")]
    public CharacterManager characterManager;

    [Tooltip("Tutti i personaggi assegnabili su questo piano")]
    public List<CharacterData> availableRoster = new List<CharacterData>();

    [Header("Root del pannello grafico da nascondere a fine selezione")]
    public GameObject teamSelectionPanelRoot;

    [Header("Pannello Disponibili")]
    public Transform availableContainer;
    public GameObject availableItemPrefab; // figli attesi: NameText, ColorImage, OrderBadge (Button sull'oggetto radice)

    [Header("Bottoni principali")]
    public Button undoButton;
    public Button confirmButton;
    public Button previewLevelButton;

    [Header("Contenitore che si nasconde con 'Vedi livello'")]
    public GameObject selectionContentRoot;

    [Header("Popup di conferma")]
    public GameObject confirmDialogRoot;
    public Button confirmDialogYesButton;
    public Button confirmDialogNoButton;

    private List<CharacterData> selectedOrder = new List<CharacterData>();
    private List<Button> availableButtons = new List<Button>();
    private List<Image> availableImages = new List<Image>();
    private List<TextMeshProUGUI> availableBadges = new List<TextMeshProUGUI>();

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

            var badge = itemGO.transform.Find("OrderBadge")?.GetComponent<TextMeshProUGUI>();
            if (badge != null) badge.gameObject.SetActive(false);

            Button button = itemGO.GetComponent<Button>();
            int index = i; // cattura locale per la closure
            button.onClick.AddListener(() => OnAvailableClicked(index));

            availableButtons.Add(button);
            availableImages.Add(colorImage);
            availableBadges.Add(badge);
        }
    }

    private void OnAvailableClicked(int index)
    {
        if (!availableButtons[index].interactable) return; // già selezionato

        CharacterData data = availableRoster[index];
        selectedOrder.Add(data);

        SetAvailableItemSelected(index, true, selectedOrder.Count);
        RefreshUI();
    }

    private void OnUndoClicked()
    {
        if (selectedOrder.Count == 0) return;

        CharacterData last = selectedOrder[selectedOrder.Count - 1];
        selectedOrder.RemoveAt(selectedOrder.Count - 1);

        int index = availableRoster.IndexOf(last);
        if (index >= 0) SetAvailableItemSelected(index, false, 0);

        RefreshUI();
    }

    private void SetAvailableItemSelected(int index, bool selected, int orderNumber)
    {
        availableButtons[index].interactable = !selected;

        if (availableImages[index] != null)
            availableImages[index].color = selected ? Color.gray : availableRoster[index].characterColor;

        if (availableBadges[index] != null)
        {
            availableBadges[index].gameObject.SetActive(selected);
            if (selected)
                availableBadges[index].text = orderNumber.ToString();
        }
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