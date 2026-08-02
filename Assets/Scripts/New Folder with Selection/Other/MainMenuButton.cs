using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MainMenuButton : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        var animator = GetComponent<UIButtonAnimator>();

        if (animator != null)
        {
            animator.SetAction(GoToMainMenu);
        }
        else
        {
            GetComponent<Button>().onClick.AddListener(GoToMainMenu);
        }
    }

    private void GoToMainMenu()
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