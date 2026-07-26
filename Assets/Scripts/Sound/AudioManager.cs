using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;
using FMOD;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("Scene names")]
    [SerializeField] private string menuSceneName = "MainMenu";

    [Header("Music")]
    [SerializeField] private EventReference menuMusic;
    [SerializeField] private EventReference gameplayMusic;

    private EventInstance musicInstance;
    private EventReference currentMusic;
    private bool musicStarted;

    private Bus SFXBus;
    private Bus musicBus;

    private void Awake()
    {
        // Ogni scena può averlo, ma ne sopravvive solo uno
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Imposta musica iniziale in base alla scena attiva
        UpdateMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMusicForScene(scene.name);
    }

    private void UpdateMusicForScene(string sceneName)
    {
        if (sceneName == menuSceneName)
            PlayMusic(menuMusic);
        else
            PlayMusic(gameplayMusic);
    }

    public void PlayMusic(EventReference newMusic)
    {
        if (newMusic.IsNull) return;

        // Se è già quella attuale, non riavviare
        if (musicStarted && newMusic.Equals(currentMusic))
            return;

        // Stop vecchia
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
            musicInstance.clearHandle();
        }

        // Start nuova
        musicInstance = RuntimeManager.CreateInstance(newMusic);
        if (!musicInstance.isValid())
        {
            UnityEngine.Debug.LogError("Music event invalid/not found");
            musicStarted = false;
            return;
        }

        musicInstance.start();
        currentMusic = newMusic;
        musicStarted = true;
    }

    public static void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public void SetSFXVolume(float value) => SFXBus.setVolume(value);
    public void SetMusicVolume(float value) => musicBus.setVolume(value);

    private void OnDestroy()
    {
        // Pulisce solo l'istanza singleton vera
        if (instance != this) return;

        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
            musicInstance.clearHandle();
        }

        instance = null;
    }
}