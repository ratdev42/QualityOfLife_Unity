using UnityEngine;
using RD42.Utils;
public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    public static T Instance { get { return instance; } }

    [Header("Singleton settings")]
    [Space]
    [SerializeField] private bool autoUnparentOnAwake = false;
    [SerializeField] private bool dontDestroyOnLoad = true;

    private static T instance;

    virtual protected void Awake()
    {
        if (Instance != null)
        {
#if DEBUG
            string typename = typeof(T).Name;
            Rd42Log.LogError(this, $"More that one instance of {typename} found.");
#endif
            Destroy(this);
            return;
        }

        instance = this as T;

        if (autoUnparentOnAwake)
        {
            transform.SetParent(null);
        }

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}