using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Panel")]
    public GameObject mainPanel;
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Settings")]
    public SettingsUI settingsUI;

    void Start()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        settingsUI.onBack = () => mainPanel.SetActive(true);

        mainPanel.SetActive(true);
    }

    private void OnPlayClicked()
    {
        SceneManager.LoadScene("Bootstrap");
    }

    private void OnSettingsClicked()
    {
        mainPanel.SetActive(false);
        settingsUI.Open();
    }

    private void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}