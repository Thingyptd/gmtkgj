using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance { get; private set; }

    public GameObject panelRoot;
    public Button resumeButton;
    public Button restartButton;
    public Button quitButton;

    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    void Awake()
    {
        Instance = this;

        panelRoot.SetActive(false);

        resumeButton.onClick.AddListener(OnResumeClicked);
        restartButton.onClick.AddListener(OnRestartClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        panelRoot.SetActive(true);
    }

    private void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        panelRoot.SetActive(false);
    }

    private void OnResumeClicked()
    {
        Resume();
    }

    private void OnRestartClicked()
    {
        Time.timeScale = 1f;

        if (ScreenTransition.Instance != null)
        {
            ScreenTransition.Instance.Close(() => GameSession.Instance.ResetCurrentFloor());
        }
        else
        {
            GameSession.Instance.ResetCurrentFloor();
        }
    }

    private void OnQuitClicked()
    {
        Time.timeScale = 1f;

        if (ScreenTransition.Instance != null)
        {
            ScreenTransition.Instance.Close(() => SceneManager.LoadScene(mainMenuSceneName));
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}