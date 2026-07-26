using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODEvents : MonoBehaviour
{
    [Serializable]
    public class PlayerSoundsHolder
    {
        public EventReference Movement;
    }

    [Serializable]
    public class UISoundsHolder
    {
        public EventReference ButtonClick;
        public EventReference Hover;
        public EventReference Pause;
    }

    [Serializable]
    public class SFXHolder
    {
        public EventReference Smash;
        public EventReference Laser;
    }

    public PlayerSoundsHolder PlayerSounds = new();
    public UISoundsHolder UISounds = new();
    public SFXHolder SFX = new();
    public static FMODEvents Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one FMOD Events instance in the scene.");
            return;
        }
        Instance = this;
    }

    public void PlayMovementSound()
    {
        RuntimeManager.PlayOneShot(PlayerSounds.Movement);
    }
 
    public void PlaylaserSound()
    {
        RuntimeManager.PlayOneShot(SFX.Laser);
    }
}