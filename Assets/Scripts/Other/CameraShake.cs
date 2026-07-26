using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Impulse Source")]
    public CinemachineImpulseSource impulseSource;

    [Header("Presets")]
    public float blastForce = 1.5f;
    public float boulderPushForce = 0.5f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (impulseSource == null)
            impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shake(float force)
    {
        if (impulseSource == null) return;
        impulseSource.GenerateImpulse(force);
    }

    public void ShakeBlast() => Shake(blastForce);
    public void ShakeBoulderPush() => Shake(boulderPushForce);
}