using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class FmodEventsEntries : ScriptableObject
{
    [field: SerializeField] public Dictionary<FmodEvent, EventReference> Cache { get; private set; }
}