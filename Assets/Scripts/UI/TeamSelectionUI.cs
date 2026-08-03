using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectionUI : MonoBehaviour
{
    public CharacterManager characterManager;

    public List<CharacterData> availableRoster = new List<CharacterData>();

    public GameObject teamSelectionPanelRoot;
    public PanelTransition teamSelectionPanelTransition;

    public Transform availableContainer;
    public GameObject availableItemPrefab;

    public Button undoButton;
    public Button confirmButton;
    public Button previewLevelButton;

    public GameObject selectionContentRoot;
    public GameObject backgroundOverlay;

    public GameObject confirmDialogRoot;
    public Button confirmDialogYesButton;
    public Button confirmDialogNoButton;

    public float levelRevealDuration = 2.5f;
    public float staggerItemDelay = 0.06f;
    public float staggerItemDuration = 0.25f;

    private List<CharacterData> selectedOrder = new List<CharacterData>();
    private List<int> selectedIndices = new List<int>();
    private List<Button> availableButtons = new List<Button>();
    private List<Image> availableImages = new List<Image>();
    private List<TextMeshProUGUI> availableBadges = new List<TextMeshProUGUI>();
    private List<SelectionItemAnimator> availableAnimators = new List<SelectionItemAnimator>();

    private bool contentVisible = true;

    void Start()
    {
        ShuffleAvailableRoster();

        bool skipSelection = ShouldSkipSelection();

        if (!skipSelection)
            BuildAvailablePanel();

        undoButton.onClick.AddListener(OnUndoClicked);
        confirmButton.onClick.AddListener(OnConfirmClicked);
        previewLevelButton.onClick.AddListener(OnPreviewToggle);
        confirmDialogYesButton.onClick.AddListener(OnConfirmDialogYes);
        confirmDialogNoButton.onClick.AddListener(OnConfirmDialogNo);

        confirmDialogRoot.SetActive(false);
        teamSelectionPanelRoot.SetActive(false);

        RefreshUI();

        StartCoroutine(RevealLevelThenShowSelection(skipSelection));
    }

    private void ShuffleAvailableRoster()
    {
        for (int i = availableRoster.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (availableRoster[i], availableRoster[j]) = (availableRoster[j], availableRoster[i]);
        }
    }

    private bool ShouldSkipSelection()
    {
        if (availableRoster.Count <= 1)
            return true;

        CharacterData first = availableRoster[0];
        for (int i = 1; i < availableRoster.Count; i++)
        {
            if (availableRoster[i] != first)
                return false;
        }

        return true;
    }

    private IEnumerator RevealLevelThenShowSelection(bool skipSelection)
    {
        yield return new WaitForSeconds(levelRevealDuration);

        if (skipSelection)
        {
            AutoConfirmSelection();
            yield break;
        }

        teamSelectionPanelRoot.SetActive(true);
        teamSelectionPanelTransition.Show(() =>
        {
            UIAnimations.FadeChildrenIn(availableContainer, staggerItemDelay, staggerItemDuration);
        });
    }

    private void AutoConfirmSelection()
    {
        GameSession.Instance.equippedCharacters = new List<CharacterData>(availableRoster);
        GameSession.Instance.currentCharacterIndex = 0;
        GameSession.Instance.currentCharacterMovesRemaining = -1;

        characterManager.BeginFloor();
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

            if (image != null)
            {
                Color c = image.color;
                c.a = 0f;
                image.color = c;
            }

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
        Debug.Log($"[TeamSelectionUI] OnAvailableClicked({index}). interactable={availableButtons[index].interactable}. selectedOrder.Count PRIMA={selectedOrder.Count}");

        if (!availableButtons[index].interactable)
        {
            Debug.LogWarning($"[TeamSelectionUI] Click ignorato su index={index}: bottone non interactable (probabilmente già selezionato).");
            return;
        }

        CharacterData data = availableRoster[index];
        selectedOrder.Add(data);
        selectedIndices.Add(index);

        Debug.Log($"[TeamSelectionUI] Selezionato index={index} ({data.characterName}). selectedIndices ora = [{string.Join(",", selectedIndices)}]");

        SetAvailableItemSelected(index, true, selectedOrder.Count);
        RefreshUI();
        FMODEvents.Instance.PlayCharacterSelection();
    }

    private void OnUndoClicked()
    {
        Debug.Log($"[TeamSelectionUI] OnUndoClicked. selectedIndices PRIMA = [{string.Join(",", selectedIndices)}]");

        if (selectedIndices.Count == 0)
        {
            Debug.LogWarning("[TeamSelectionUI] Undo premuto ma selectedIndices è vuoto, nessuna azione.");
            return;
        }

        int lastIndex = selectedIndices[selectedIndices.Count - 1];
        selectedIndices.RemoveAt(selectedIndices.Count - 1);
        selectedOrder.RemoveAt(selectedOrder.Count - 1);

        Debug.Log($"[TeamSelectionUI] Undo: deseleziono index={lastIndex}. selectedIndices DOPO = [{string.Join(",", selectedIndices)}]. Button interactable PRIMA della SetAvailableItemSelected = {availableButtons[lastIndex].interactable}");

        SetAvailableItemSelected(lastIndex, false, 0);

        Debug.Log($"[TeamSelectionUI] Undo: index={lastIndex} interactable DOPO = {availableButtons[lastIndex].interactable}");

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
        undoButton.interactable = selectedIndices.Count > 0;

        bool allSelected = selectedOrder.Count == availableRoster.Count && availableRoster.Count > 0;
        confirmButton.gameObject.SetActive(allSelected);

        Debug.Log($"[TeamSelectionUI] RefreshUI. selectedOrder.Count={selectedOrder.Count}, availableRoster.Count={availableRoster.Count}, allSelected={allSelected}, undoButton.interactable={undoButton.interactable}");
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

        if (backgroundOverlay != null)
            backgroundOverlay.SetActive(contentVisible);
    }
}