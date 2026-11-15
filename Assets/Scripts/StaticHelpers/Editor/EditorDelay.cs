using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using Object = UnityEngine.Object;

/// <summary>
/// Schedules one-shot callbacks on <see cref="EditorApplication.delayCall"/> and lets an owner
/// (EditorWindow, Editor, ScriptableObject...) cancel its pending callbacks when it is destroyed.
/// Callbacks whose owner has already been destroyed are skipped automatically.
/// </summary>
public static class EditorDelay
{
    private static readonly Dictionary<Object, List<EditorApplication.CallbackFunction>> pendingCallbacks = new (ReferenceComparer.Instance);

    /// <summary>
    /// Runs <paramref name="action"/> once on the next editor update.
    /// Use this overload when the callback doesn't belong to any object.
    /// </summary>
    public static void Call(Action action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        // Callback unsubscribes itself after running, so nothing stays registered.
#pragma warning disable UDR0004
        EditorApplication.delayCall += Callback;
#pragma warning restore UDR0004

        void Callback()
        {
            EditorApplication.delayCall -= Callback;
            action();
        }
    }

    /// <summary>
    /// Runs <paramref name="action"/> once on the next editor update, tied to <paramref name="owner"/>.
    /// Call <see cref="CancelAll"/> from the owner's OnDestroy to drop anything still pending.
    /// </summary>
    public static void Call(Object owner, Action action)
    {
        if (ReferenceEquals(owner, null)) throw new ArgumentNullException(nameof(owner));
        if (action == null) throw new ArgumentNullException(nameof(action));

        if (!pendingCallbacks.TryGetValue(owner, out var list))
        {
            list = new List<EditorApplication.CallbackFunction>();
            pendingCallbacks.Add(owner, list);
        }

        list.Add(Callback);

        // Callback unsubscribes itself after running, and the owner can cancel it via CancelAll.
#pragma warning disable UDR0004
        EditorApplication.delayCall += Callback;
#pragma warning restore UDR0004

        void Callback()
        {
            EditorApplication.delayCall -= Callback;
            Remove(owner, Callback);

            if (owner == null) return;

            action();
        }
    }

    /// <summary>
    /// Cancels every pending callback registered for <paramref name="owner"/>.
    /// Call this from OnDestroy (or OnDisable).
    /// </summary>
    public static void CancelAll(Object owner)
    {
        if (ReferenceEquals(owner, null)) return;
        if (!pendingCallbacks.TryGetValue(owner, out var list)) return;

        foreach (var callback in list)
        {
            EditorApplication.delayCall -= callback;
        }

        pendingCallbacks.Remove(owner);
    }

    private static void Remove(Object owner, EditorApplication.CallbackFunction callback)
    {
        if (!pendingCallbacks.TryGetValue(owner, out var list)) return;

        list.Remove(callback);
        if (list.Count == 0)
        {
            pendingCallbacks.Remove(owner);
        }
    }

    // UnityEngine.Object overrides Equals so that destroyed objects compare equal to null
    // (and to each other), which would break dictionary lookups. Compare by reference instead.
    private sealed class ReferenceComparer : IEqualityComparer<Object>
    {
        public static readonly ReferenceComparer Instance = new ();

        public bool Equals(Object x, Object y) => ReferenceEquals(x, y);
        public int GetHashCode(Object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}