using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODEvents : MonoBehaviour
{
    [Serializable]
    public class PlayerSoundsHolder
    {
        public EventReference Footsteps;
        public EventReference Jump;
        public EventReference Spallucce;
        public EventReference Atterraggio;
    }

    [Serializable]
    public class UISoundsHolder
    {
        public EventReference ButtonClick;
        public EventReference Hover;
        public EventReference Pause;
        public EventReference Pelly;
        public EventReference Map;
    }

    [Serializable]
    public class SFXHolder
    {
        public EventReference Interact;
        public EventReference PuzzleComplete;
        public EventReference Ingranaggio;
        public EventReference IngranaggioTrigger;
        public EventReference Trampolino;
        public EventReference Rewind;
        public EventReference Pellicola;
        public EventReference Ciak;
        public EventReference Platform;
        public EventReference PlatformStop;
        public EventReference PlatformLoop;
        public EventReference ScotchPlaced;
        public EventReference ScotchLanded;
        public EventReference FramePlaced;
        public EventReference FrameLanded;
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
}