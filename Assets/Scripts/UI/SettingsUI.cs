using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Root & Tabs")]
    public PanelTransition settingsRootTransition;
    public PanelTransition videoTabTransition;
    public PanelTransition audioTabTransition;
    public Button videoTabButton;
    public Button audioTabButton;
    public Button backButton;

    [Header("Audio Controls")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Video Controls")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;

    public System.Action onBack;

    private List<Resolution> availableResolutions;
    private bool showingVideo = true;

    void Start()
    {
        videoTabButton.GetComponent<UIButtonAnimator>().SetAction(() => SwitchTab(true));
        audioTabButton.GetComponent<UIButtonAnimator>().SetAction(() => SwitchTab(false));
        backButton.GetComponent<UIButtonAnimator>().SetAction(OnBackClicked);

        masterVolumeSlider.onValueChanged.AddListener(v => GameSettings.MasterVolume = v);
        musicVolumeSlider.onValueChanged.AddListener(v => GameSettings.MusicVolume = v);
        sfxVolumeSlider.onValueChanged.AddListener(v => GameSettings.SfxVolume = v);

        fullscreenToggle.onValueChanged.AddListener(v => GameSettings.Fullscreen = v);
        vsyncToggle.onValueChanged.AddListener(v => GameSettings.Vsync = v);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        BuildResolutionDropdown();
        LoadCurrentValuesIntoUI();

        settingsRootTransition.gameObject.SetActive(false);
        videoTabTransition.gameObject.SetActive(false);
        audioTabTransition.gameObject.SetActive(false);
    }

    private void BuildResolutionDropdown()
    {
        availableResolutions = new List<Resolution>(Screen.resolutions);
        List<string> options = new List<string>();
        foreach (var r in availableResolutions)
            options.Add($"{r.width} x {r.height} @ {Mathf.RoundToInt((float)r.refreshRateRatio.value)}Hz");

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
    }

    private void LoadCurrentValuesIntoUI()
    {
        masterVolumeSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
        musicVolumeSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
        sfxVolumeSlider.SetValueWithoutNotify(GameSettings.SfxVolume);
        fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);
        vsyncToggle.SetIsOnWithoutNotify(GameSettings.Vsync);

        int savedResIndex = GameSettings.ResolutionIndex;
        if (savedResIndex < 0) savedResIndex = FindCurrentResolutionIndex();
        resolutionDropdown.SetValueWithoutNotify(savedResIndex);
    }

    private int FindCurrentResolutionIndex()
    {
        for (int i = 0; i < availableResolutions.Count; i++)
            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height)
                return i;
        return availableResolutions.Count - 1;
    }

    private void OnResolutionChanged(int index)
    {
        GameSettings.ResolutionIndex = index;
        Resolution r = availableResolutions[index];
        Screen.SetResolution(r.width, r.height, GameSettings.Fullscreen);
    }

    public void Open()
    {
        settingsRootTransition.gameObject.SetActive(true);
        settingsRootTransition.Show(() => SwitchTab(true, instant: true));
    }

    private void SwitchTab(bool video, bool instant = false)
    {
        if (video == showingVideo && !instant) return;
        showingVideo = video;

        if (instant)
        {
            videoTabTransition.gameObject.SetActive(video);
            audioTabTransition.gameObject.SetActive(!video);
            if (video) videoTabTransition.Show(); else audioTabTransition.Show();
            return;
        }

        if (video)
        {
            audioTabTransition.Hide(() => videoTabTransition.Show());
        }
        else
        {
            videoTabTransition.Hide(() => audioTabTransition.Show());
        }
    }

    private void OnBackClicked()
    {
        GameSettings.Save();
        settingsRootTransition.Hide(() => onBack?.Invoke());
    }
}