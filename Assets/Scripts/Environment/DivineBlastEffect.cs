using UnityEngine;

public class DivineBlastEffect : MonoBehaviour
{
    private Animation anim;

    public float AnimationDuration { get; private set; }

    void Awake()
    {
        anim = GetComponentInChildren<Animation>();

        if (anim == null)
        {
            AnimationDuration = 0f;
            return;
        }

        if (anim.clip == null)
        {
            AnimationDuration = 0f;
            return;
        }

        anim.wrapMode = WrapMode.Once;
        AnimationDuration = anim.clip.length;
    }

    public void Play()
    {
        if (anim == null || anim.clip == null)
            return;

        if (!anim.gameObject.activeSelf)
            anim.gameObject.SetActive(true);

        anim.Play(anim.clip.name);
        FMODEvents.Instance.PlaylaserSound();

        if (CameraShake.Instance != null)
            CameraShake.Instance.ShakeBlast();
    }
}