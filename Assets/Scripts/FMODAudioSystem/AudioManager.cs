using FMODUnity;
using RD42.Utils;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    private CustomLogger logger;
    protected override void Awake()
    {
        base.Awake();

        logger = new(this, "AudioManager", Color.orange);
    }

    public void PlayOneShot(EventReference soundRef, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(soundRef, worldPos);
    }
}
