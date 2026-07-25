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

        if (spriteRenderer != null && frame1 != null)
            spriteRenderer.sprite = frame1;
    }

    void Update()
    {
        if (!idleEnabled || spriteRenderer == null) return;

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

        GameObject fx = Instantiate(moveParticlePrefab, worldPos, Quaternion.identity);

        if (particleLifetime > 0f)
            Destroy(fx, particleLifetime);
    }
}