using FMOD.Studio;
using FMODUnity;
using UnityEngine;
public class MovementAnimations : MonoBehaviour
{
    [Header("Move Particle")]
    public GameObject moveParticlePrefab;
    public float particleLifetime = 1f;
    [Header("Idle Animation")]
    [Tooltip("Secondi tra un frame e l'altro dell'idle")]
    public float idleFrameDuration = 0.4f;
    private SpriteRenderer spriteRenderer;
    private Sprite frame1;
    private Sprite frame2;
    private float idleTimer;
    private bool showingFrame1 = true;
    private bool idleEnabled = false;

    private Sprite sneakFrame1;
    private Sprite sneakFrame2;
    private float sneakFrameDuration = 0.08f;
    private float sneakTimer;
    private bool showingSneakFrame1 = true;
    private bool isSneaking = false;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    /// <summary>Chiamato da GridMovement.Initialize() per impostare i 2 frame di questo personaggio.</summary>
    public void SetIdleFrames(Sprite f1, Sprite f2)
    {
        frame1 = f1;
        frame2 = f2;
        idleTimer = 0f;
        showingFrame1 = true;
        idleEnabled = (f1 != null && f2 != null);
        if (spriteRenderer != null && frame1 != null && !isSneaking)
            spriteRenderer.sprite = frame1;
    }

    /// <summary>Chiamato da GridMovement.Initialize() per impostare i 2 frame dell'animazione sneak.</summary>
    public void SetSneakFrames(Sprite f1, Sprite f2, float frameDuration)
    {
        sneakFrame1 = f1;
        sneakFrame2 = f2;
        sneakFrameDuration = frameDuration;
    }

    public void StartSneaking()
    {
        if (isSneaking) return;
        if (sneakFrame1 == null || sneakFrame2 == null) return;

        isSneaking = true;
        sneakTimer = 0f;
        showingSneakFrame1 = true;

        if (spriteRenderer != null)
            spriteRenderer.sprite = sneakFrame1;
        StartSneakSound();
    }

    public void StopSneaking()
    {
        if (!isSneaking) return;

        isSneaking = false;
        idleTimer = 0f;
        showingFrame1 = true;

        if (spriteRenderer != null && idleEnabled && frame1 != null)
            spriteRenderer.sprite = frame1;
        StopSneakSound();
    }

    void Update()
    {
        if (spriteRenderer == null) return;

        if (isSneaking)
        {
            sneakTimer += Time.deltaTime;
            if (sneakTimer >= sneakFrameDuration)
            {
                sneakTimer = 0f;
                showingSneakFrame1 = !showingSneakFrame1;
                spriteRenderer.sprite = showingSneakFrame1 ? sneakFrame1 : sneakFrame2;
            }
            return;
        }

        if (!idleEnabled) return;
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleFrameDuration)
        {
            idleTimer = 0f;
            showingFrame1 = !showingFrame1;
            spriteRenderer.sprite = showingFrame1 ? frame1 : frame2;
        }
    }
    public void PlayMoveParticle(Vector3 worldPos)
    {
        if (moveParticlePrefab == null) return;
        FMODEvents.Instance.PlayMovementSound();
        GameObject fx = Instantiate(moveParticlePrefab, worldPos, Quaternion.identity);
        if (particleLifetime > 0f)
            Destroy(fx, particleLifetime);
    }

    private EventInstance sneakSound;

    private void StartSneakSound()
    {
        sneakSound = RuntimeManager.CreateInstance(FMODEvents.Instance.PlayerSounds.Sneak);
        sneakSound.start();
    }

    private void StopSneakSound()
    {
        if (sneakSound.isValid())
        {
            sneakSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            sneakSound.release();
        }
    }
}