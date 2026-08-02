using UnityEngine;
using UnityEngine.InputSystem;
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
        if (Instance != null && Instance != this)
        {
            Destroy(transform.root.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject);

        if (panelRoot == null)
        {
            Debug.LogError($"[PauseMenuUI:{name}] 'Panel Root' non è assegnato!");
        }
        else
        {
            panelRoot.SetActive(false);
        }

        if (resumeButton == null || restartButton == null || quitButton == null)
        {
            Debug.LogError($"[PauseMenuUI:{name}] Uno dei bottoni (Resume/Restart/Quit) non è assegnato!");
        }
        else
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
            restartButton.onClick.AddListener(OnRestartClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
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

        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    private void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnResumeClicked()
    {
        Resume();
    }

    private void OnRestartClicked()
    {
        ClosePauseStateBeforeSceneChange();

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
        ClosePauseStateBeforeSceneChange();

        if (ScreenTransition.Instance != null)
        {
            ScreenTransition.Instance.Close(() => SceneManager.LoadScene(mainMenuSceneName));
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    /// <summary>
    /// Dato che questo oggetto è persistente (DontDestroyOnLoad), sopravvive a qualunque
    /// cambio scena — quindi il pannello e lo stato "in pausa" vanno resettati esplicitamente
    /// PRIMA di ricaricare qualunque scena, altrimenti restano visibili/attivi anche dopo.
    /// </summary>
    private void ClosePauseStateBeforeSceneChange()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}