using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using FMOD;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    private EventInstance ambianceEventInstance;
    [field: SerializeField] public EventReference ambience { get; private set; }

    private EventInstance musicInstance;
    [field: Header("Music")]
    [field: SerializeField] public EventReference music { get; private set; }

    private Bus SFXBus;
    private Bus musicBus;

    private void Awake()
    {
        // Singleton robusto: tieni solo la prima istanza
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SFXBus = RuntimeManager.GetBus("bus:/SFX");
        musicBus = RuntimeManager.GetBus("bus:/MUSIC");
    }

    private void Start()
    {
        InitializeAmbience(ambience);
        InitializeMusic(music); // avvia una sola volta
    }

    public static void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    private void InitializeAmbience(EventReference ambienceReference)
    {
        if (ambianceEventInstance.isValid()) return; // evita doppie istanze

        ambianceEventInstance = RuntimeManager.CreateInstance(ambienceReference);
        ambianceEventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));

        if (!ambianceEventInstance.isValid())
        {
            UnityEngine.Debug.LogError("Ambience event not found");
            return;
        }

        ambianceEventInstance.start();
    }

    private void InitializeMusic(EventReference eventReference)
    {
        if (musicInstance.isValid()) return; // evita doppie istanze

        musicInstance = RuntimeManager.CreateInstance(eventReference);

        if (!musicInstance.isValid())
        {
            UnityEngine.Debug.LogError("Music event not found");
            return;
        }

        musicInstance.start();
    }

    public void StopMusic()
    {
        if (!musicInstance.isValid()) return;
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
        musicInstance.clearHandle();
    }

    public void SetSFXVolume(float value)
    {
        SFXBus.setVolume(value);
        UnityEngine.Debug.Log($"SFX Volume set to: {value}");
    }

    public void SetMusicVolume(float value)
    {
        musicBus.setVolume(value);
    }

    public float GetSFXVolume()
    {
        SFXBus.getVolume(out float sfxVolume);
        return sfxVolume;
    }

    public float GetMusicVolume()
    {
        musicBus.getVolume(out float musicVolume);
        return musicVolume;
    }

    private void OnDestroy()
    {
        // Solo l'istanza vera fa cleanup
        if (instance != this) return;

        if (ambianceEventInstance.isValid())
        {
            ambianceEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            ambianceEventInstance.release();
        }

        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }

        instance = null;
    }
}