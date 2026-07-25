using UnityEngine;

/// <summary>
/// Raccoglie tutte le animazioni/effetti visivi legati al movimento del personaggio.
/// GridMovement chiama questi metodi nei momenti giusti, senza sapere come sono implementati.
/// </summary>
public class MovementAnimations : MonoBehaviour
{
    [Header("Move Particle")]
    public GameObject moveParticlePrefab;
    public float particleLifetime = 1f;

    public void PlayMoveParticle(Vector3 worldPos)
    {
        if (moveParticlePrefab == null) return;

        GameObject fx = Instantiate(moveParticlePrefab, worldPos, Quaternion.identity);

        if (particleLifetime > 0f)
            Destroy(fx, particleLifetime);
    }
}