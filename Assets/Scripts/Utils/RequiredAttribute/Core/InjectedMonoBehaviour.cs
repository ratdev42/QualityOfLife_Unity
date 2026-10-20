using UnityEngine;

public class InjectedMonoBehaviour : MonoBehaviour
{
    protected virtual void Awake()
    {
        ComponentInjector.InjectRequired(this);
    }
}
