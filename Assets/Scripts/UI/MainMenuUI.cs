using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Panel")]
    public PanelTransition mainPanelTransition;
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Settings")]
    public SettingsUI settingsUI;

    void Start()
    {
        playButton.GetComponent<UIButtonAnimator>().SetAction(OnPlayClicked);
        settingsButton.GetComponent<UIButtonAnimator>().SetAction(OnSettingsClicked);
        quitButton.GetComponent<UIButtonAnimator>().SetAction(OnQuitClicked);

        settingsUI.onBack = () => mainPanelTransition.Show();

        mainPanelTransition.gameObject.SetActive(true);
        mainPanelTransition.Show();
    }

    private void OnPlayClicked()
    {
        FMODEvents.Instance.PlayStartSound();
        SceneManager.LoadScene("Bootstrap");
    }

    private void OnSettingsClicked()
    {
        mainPanelTransition.Hide(() => settingsUI.Open());
    }

    private void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}