using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Gestisce lettura/scrittura delle impostazioni persistenti (PlayerPrefs) e la loro applicazione al motore.
/// Nessuna UI qui dentro: SettingsUI legge/scrive attraverso questa classe.
/// </summary>
public static class GameSettings
{
    // --- Chiavi PlayerPrefs ---
    private const string MasterVolumeKey = "audio_master";
    private const string MusicVolumeKey = "audio_music";
    private const string SfxVolumeKey = "audio_sfx";

    private const string ResolutionIndexKey = "video_resolution_index";
    private const string FullscreenKey = "video_fullscreen";
    private const string QualityLevelKey = "video_quality";
    private const string VsyncKey = "video_vsync";

    // --- Audio ---
    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        set { PlayerPrefs.SetFloat(MasterVolumeKey, value); AudioListener.volume = value; }
    }

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        set => PlayerPrefs.SetFloat(MusicVolumeKey, value);
        // Se in futuro usi un AudioMixer, qui dentro faresti anche
        // mixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        set => PlayerPrefs.SetFloat(SfxVolumeKey, value);
    }

    // --- Video ---
    public static bool Fullscreen
    {
        get => PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        set { PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0); Screen.fullScreen = value; }
    }

    public static int QualityLevel
    {
        get => PlayerPrefs.GetInt(QualityLevelKey, QualitySettings.GetQualityLevel());
        set { PlayerPrefs.SetInt(QualityLevelKey, value); QualitySettings.SetQualityLevel(value, true); }
    }

    public static bool Vsync
    {
        get => PlayerPrefs.GetInt(VsyncKey, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
        set { PlayerPrefs.SetInt(VsyncKey, value ? 1 : 0); QualitySettings.vSyncCount = value ? 1 : 0; }
    }

    public static int ResolutionIndex
    {
        get => PlayerPrefs.GetInt(ResolutionIndexKey, -1); // -1 = non ancora impostato
        set => PlayerPrefs.SetInt(ResolutionIndexKey, value);
    }

    /// <summary>Applica TUTTE le impostazioni salvate. Chiamalo una volta all'avvio del gioco (es. in Bootstrap).</summary>
    public static void ApplyAll()
    {
        AudioListener.volume = MasterVolume;
        Screen.fullScreen = Fullscreen;
        QualitySettings.SetQualityLevel(QualityLevel, true);
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