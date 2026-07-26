using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectionUI : MonoBehaviour
{
    public CharacterManager characterManager;

    public List<CharacterData> availableRoster = new List<CharacterData>();

    public GameObject teamSelectionPanelRoot;

    public Transform availableContainer;
    public GameObject availableItemPrefab;

    public Button undoButton;
    public Button confirmButton;
    public Button previewLevelButton;

    public GameObject selectionContentRoot;

    public GameObject confirmDialogRoot;
    public Button confirmDialogYesButton;
    public Button confirmDialogNoButton;

    private List<CharacterData> selectedOrder = new List<CharacterData>();
    private List<Button> availableButtons = new List<Button>();
    private List<Image> availableImages = new List<Image>();
    private List<TextMeshProUGUI> availableBadges = new List<TextMeshProUGUI>();
    private List<SelectionItemAnimator> availableAnimators = new List<SelectionItemAnimator>();

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
        teamSelectionPanelRoot.SetActive(true);

        RefreshUI();
    }

    private void BuildAvailablePanel()
    {
        for (int i = 0; i < availableRoster.Count; i++)
        {
            CharacterData data = availableRoster[i];

            GameObject itemGO = Instantiate(availableItemPrefab, availableContainer);

            Image image = itemGO.GetComponent<Image>();
            if (image != null)
                image.sprite = data.selectionIcon;

            var badge = itemGO.transform.Find("OrderBadge")?.GetComponent<TextMeshProUGUI>();
            if (badge != null)
                badge.gameObject.SetActive(false);

            var animator = itemGO.GetComponent<SelectionItemAnimator>();
            if (animator == null)
                animator = itemGO.AddComponent<SelectionItemAnimator>();

            Button button = itemGO.GetComponent<Button>();
            int index = i;
            animator.SetAction(() => OnAvailableClicked(index));

            availableButtons.Add(button);
            availableImages.Add(image);
            availableBadges.Add(badge);
            availableAnimators.Add(animator);
        }
    }

    private void OnAvailableClicked(int index)
    {
        if (!availableButtons[index].interactable) return;

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
        availableAnimators[index].SetLocked(selected);

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