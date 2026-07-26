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
        public EventReference Select;
        public EventReference Hover;
        public EventReference Pause;
        public EventReference Start;
    }

    [Serializable]
    public class SFXHolder
    {
        public EventReference Boulder;
        public EventReference Laser;
        public EventReference Fall;
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

    public void PlaySelectSound()
    {
        RuntimeManager.PlayOneShot(UISounds.Select);
    }

    public void PlayHoverSound()
    {
        RuntimeManager.PlayOneShot(UISounds.Hover);
    }

    public void PlayStartSound()
    {
        RuntimeManager.PlayOneShot(UISounds.Start);
    }

    public void PlayFallSound()
    {
        RuntimeManager.PlayOneShot(SFX.Fall);
    }

    public void PlayBoulderSound()
    {
        RuntimeManager.PlayOneShot(SFX.Boulder);
    }
}