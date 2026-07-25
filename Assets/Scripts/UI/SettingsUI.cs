using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Root & Tabs")]
    public GameObject settingsRoot;
    public GameObject videoTabPanel;
    public GameObject audioTabPanel;
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

    void Start()
    {
        videoTabButton.onClick.AddListener(() => ShowTab(true));
        audioTabButton.onClick.AddListener(() => ShowTab(false));
        backButton.onClick.AddListener(OnBackClicked);

        masterVolumeSlider.onValueChanged.AddListener(v => GameSettings.MasterVolume = v);
        musicVolumeSlider.onValueChanged.AddListener(v => GameSettings.MusicVolume = v);
        sfxVolumeSlider.onValueChanged.AddListener(v => GameSettings.SfxVolume = v);

        fullscreenToggle.onValueChanged.AddListener(v => GameSettings.Fullscreen = v);
        vsyncToggle.onValueChanged.AddListener(v => GameSettings.Vsync = v);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        BuildResolutionDropdown();
        LoadCurrentValuesIntoUI();

        ShowTab(true);
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
        if (savedResIndex < 0)
            savedResIndex = FindCurrentResolutionIndex();

        resolutionDropdown.SetValueWithoutNotify(savedResIndex);
    }

    private int FindCurrentResolutionIndex()
    {
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height)
                return i;
        }
        return availableResolutions.Count - 1;
    }

    private void OnResolutionChanged(int index)
    {
        GameSettings.ResolutionIndex = index;
        Resolution r = availableResolutions[index];
        Screen.SetResolution(r.width, r.height, GameSettings.Fullscreen);
    }

    private void ShowTab(bool video)
    {
        videoTabPanel.SetActive(video);
        audioTabPanel.SetActive(!video);
    }

    private void OnBackClicked()
    {
        GameSettings.Save();
        settingsRoot.SetActive(false);
        onBack?.Invoke();
    }

    public void Open()
    {
        settingsRoot.SetActive(true);
        LoadCurrentValuesIntoUI();
    }
}