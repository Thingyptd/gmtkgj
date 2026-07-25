using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public static class GameSettings
{
    // --- Chiavi PlayerPrefs ---
    private const string MasterVolumeKey = "audio_master";
    private const string MusicVolumeKey = "audio_music";
    private const string SfxVolumeKey = "audio_sfx";
    private const string ResolutionIndexKey = "video_resolution_index";
    private const string FullscreenKey = "video_fullscreen";
    private const string VsyncKey = "video_vsync";

    // --- Audio (via FMOD VCA) ---
    private static VCA masterVCA;
    private static VCA musicVCA;
    private static VCA sfxVCA;

    private static void EnsureVCAsLoaded()
    {
        //if (!masterVCA.isValid()) masterVCA = RuntimeManager.GetVCA("vca:/Master");
        //if (!musicVCA.isValid()) musicVCA = RuntimeManager.GetVCA("vca:/Music");
        //if (!sfxVCA.isValid()) sfxVCA = RuntimeManager.GetVCA("vca:/SFX");
    }

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, value);
            EnsureVCAsLoaded();
            masterVCA.setVolume(value);
        }
    }

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
            EnsureVCAsLoaded();
            musicVCA.setVolume(value);
        }
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(SfxVolumeKey, value);
            EnsureVCAsLoaded();
            sfxVCA.setVolume(value);
        }
    }

    // --- Video ---
    public static bool Fullscreen
    {
        get => PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        set { PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0); Screen.fullScreen = value; }
    }

    public static bool Vsync
    {
        get => PlayerPrefs.GetInt(VsyncKey, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
        set { PlayerPrefs.SetInt(VsyncKey, value ? 1 : 0); QualitySettings.vSyncCount = value ? 1 : 0; }
    }

    public static int ResolutionIndex
    {
        get => PlayerPrefs.GetInt(ResolutionIndexKey, -1);
        set => PlayerPrefs.SetInt(ResolutionIndexKey, value);
    }

    public static void ApplyAll()
    {
        EnsureVCAsLoaded();
        masterVCA.setVolume(MasterVolume);
        musicVCA.setVolume(MusicVolume);
        sfxVCA.setVolume(SfxVolume);

        Screen.fullScreen = Fullscreen;
        QualitySettings.vSyncCount = Vsync ? 1 : 0;

        int resIndex = ResolutionIndex;
        if (resIndex >= 0 && resIndex < Screen.resolutions.Length)
        {
            Resolution r = Screen.resolutions[resIndex];
            Screen.SetResolution(r.width, r.height, Fullscreen);
        }
    }

    public static void Save() => PlayerPrefs.Save();
}