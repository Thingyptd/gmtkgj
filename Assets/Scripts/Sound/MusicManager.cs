using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private EventInstance currentMusicInstance;
    private EventReference currentEvent { get; set; }
    private bool hasMusic;

    private void Awake()
    {
        // Distruggi duplicati tra scene
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(EventReference musicEvent)
    {
        // Se è già la stessa musica in play, non rifare nulla
        if (hasMusic && musicEvent.Equals(currentEvent))
            return;

        StopMusic();

        currentEvent = musicEvent;
        currentMusicInstance = RuntimeManager.CreateInstance(currentEvent);
        currentMusicInstance.start();
        hasMusic = true;
    }

    public void SetParameter(string parameterName, float value)
    {
        if (!hasMusic) return;
        currentMusicInstance.setParameterByName(parameterName, value);
    }

    public void StopMusic()
    {
        if (!hasMusic) return;

        currentMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        currentMusicInstance.release(); // importante: libera la handle FMOD
        hasMusic = false;
    }

    private void OnDestroy()
    {
        // cleanup quando l'app chiude o se l'unica istanza viene distrutta
        if (Instance == this)
        {
            StopMusic();
            Instance = null;
        }
    }
}