using FMODUnity;
using UnityEngine;

public class FmodEvents : Singleton<FmodEvents>
{
    [SerializeField] private FmodEventsEntries entries;

    public EventReference Get(FmodEvent fmodEvent)
    {
        return entries.Cache[fmodEvent];
    }
}
