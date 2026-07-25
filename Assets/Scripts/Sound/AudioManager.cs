using System.Collections;
using System.Collections.Generic;
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
        if (instance != null)
        {
            UnityEngine.Debug.LogError("Found more than one Audio Manager in the scene.");
        }
        instance = this;
        SFXBus = RuntimeManager.GetBus("bus:/SFX");
        musicBus = RuntimeManager.GetBus("bus:/MUSIC");

    }
    public static void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    private void Start()
    {
        InitializeAmbience(ambience);
        InitializeMusic(music);
    }
    private void InitializeAmbience(EventReference ambienceReference)
    {
        ambianceEventInstance = RuntimeManager.CreateInstance(ambienceReference);
        ambianceEventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        if (!ambianceEventInstance.isValid())
        {
            UnityEngine.Debug.LogError("Ambience event not found");
            return;
        }
        ambianceEventInstance.start();
    }

    public void OnDestroy()
    {
        ambianceEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        ambianceEventInstance.release();
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
    }

    private void InitializeMusic(EventReference eventReference)
    {
        musicInstance = RuntimeManager.CreateInstance(eventReference);
        musicInstance.start();
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

    /*
       public void GetVolumes()
       {
           var sfxVolume = 0f;
           var musicVolume = 0f;
           SFXBus.getVolume(out sfxVolume);
           musicBus.getVolume(out musicVolume);
       }*/

    public float GetSFXVolume()
    {
        var sfxVolume = 0f;
        SFXBus.getVolume(out sfxVolume);
        return sfxVolume;
    }

    public float GetMusicVolume()
    {
        var musicVolume = 0f;
        musicBus.getVolume(out musicVolume);
        return musicVolume;
    }
}
