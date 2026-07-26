using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneUI : MonoBehaviour
{
    public List<Sprite> slides = new List<Sprite>();

    public Image slideImage;
    public Button nextButton;
    public Button skipButton;

    public float fadeDuration = 0.3f;

    public string nextSceneName = "MainMenu";

    public event Action OnCutsceneComplete;

    private int slideIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        nextButton.onClick.AddListener(OnNextClicked);

        if (slides.Count == 0)
        {
            EndCutscene();
            return;
        }

        ShowSlide(slideIndex);
    }

    private void ShowSlide(int index)
    {
        slideImage.sprite = slides[index];

        Color c = slideImage.color;
        c.a = 1f;
        slideImage.color = c;
    }

    private void OnNextClicked()
    {
        if (isTransitioning) return;

        StartCoroutine(AdvanceSlide());
    }

    private IEnumerator AdvanceSlide()
    {
        isTransitioning = true;

        slideIndex++;

        if (slideIndex >= slides.Count)
        {
            EndCutscene();
            yield break;
        }

        yield return slideImage.DOFade(0f, fadeDuration).WaitForCompletion();

        ShowSlide(slideIndex);

        yield return slideImage.DOFade(1f, fadeDuration).WaitForCompletion();

        isTransitioning = false;
    }

    private void OnSkipClicked()
    {
        StopAllCoroutines();
        EndCutscene();
    }

    private void EndCutscene()
    {
        gameObject.SetActive(false);
        OnCutsceneComplete?.Invoke();

        if (ScreenTransition.Instance != null)
        {
            ScreenTransition.Instance.Close(() => SceneManager.LoadScene(nextSceneName));
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}