using FMODUnity;
using RD42.Utils;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private FmodEventsEntries entries;

    private CustomLogger logger;
    protected override void Awake()
    {
        base.Awake();

        logger = new(this, "AudioManager", Color.orange);
    }

    public void PlayOneShot(FmodEvent soundEvent, Vector3 worldPos)
    {
        EventReference soundRef = entries.Cache[soundEvent];
        RuntimeManager.PlayOneShot(soundRef, worldPos);
    }
}
