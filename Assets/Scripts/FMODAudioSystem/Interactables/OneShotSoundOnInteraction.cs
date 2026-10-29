using FMODUnity;
using RD42.Utils;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OneShotSoundOnInteraction : InjectedMonoBehaviour, IInteractable
{
    [SerializeField] private FmodEvent soundEvent;
    [Required] private readonly new Collider collider;

    private CustomLogger logger;

    protected override void Awake()
    {
        base.Awake();

        collider.isTrigger = true;
        logger = new(this, "OnShotSoundInteraction", Color.cyan);
    }

    private void OnTriggerEnter(Collider other)
    {
        logger.Info($"Collided with {other.name}");
        OnInteraction();
    }

    public void OnInteraction()
    {
        AudioManager.Instance.PlayOneShot(FmodEvents.Instance.Get(soundEvent), transform.position);
    }
}
