using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class ComponentInjector
{
    private static readonly Dictionary<Type, (FieldInfo field, RequiredAttribute attribute)[]> Cache = new();

    public static void InjectRequired(this MonoBehaviour target)
    {
        (FieldInfo field, RequiredAttribute attribute)[] entries = GetEntries(target.GetType());

        foreach (var (field, attribute) in entries)
        {
            bool isArray = field.FieldType.IsArray;

            object value = isArray
                ? ResolveArray(target, field.FieldType.GetElementType(), attribute)
                : Resolve(target, field.FieldType, attribute);

            bool isMissing = isArray ? ((Array)value).Length == 0 : value == null;

            if (isMissing && !attribute.Optional)
            {
                Debug.LogError(
                    $"[{target.GetType().Name}] Required {field.FieldType.Name} " +
                    $"not found ({attribute.Search}) on '{target.name}'.", target);
                continue;
            }

            field.SetValue(target, value);
        }
    }

    private static Component Resolve(Component target, Type type, RequiredAttribute attribute)
    {
        return attribute.Search switch
        {
            Search.Self => target.GetComponent(type),
            Search.Children => target.GetComponentInChildren(type, attribute.IncludeInactive),
            Search.Parent => target.GetComponentInParent(type, attribute.IncludeInactive),
            Search.ChildrenOnly => target.GetComponentsInChildren(type, attribute.IncludeInactive).FirstOrDefault(component => component.gameObject != target.gameObject),
            Search.ParentOnly => target.transform.parent == null
                                ? null
                                : target.transform.parent.GetComponentInParent(type, attribute.IncludeInactive),
            _ => null,
        };
    }

    private static Array ResolveArray(Component target, Type elementType, RequiredAttribute attribute)
    {
        Component[] components = attribute.Search switch
        {
            Search.Self => target.GetComponents(elementType),
            Search.Children => target.GetComponentsInChildren(elementType, attribute.IncludeInactive),
            Search.Parent => target.GetComponentsInParent(elementType, attribute.IncludeInactive),
            Search.ChildrenOnly => target.GetComponentsInChildren(elementType, attribute.IncludeInactive)
                                        .Where(component => component.gameObject != target.gameObject).ToArray(),
            Search.ParentOnly => target.transform.parent == null
                                ? Array.Empty<Component>()
                                : target.transform.parent.GetComponentsInParent(elementType, attribute.IncludeInactive),
            _ => Array.Empty<Component>(),
        };

        Array typedArray = Array.CreateInstance(elementType, components.Length);
        Array.Copy(components, typedArray, components.Length);

        return typedArray;
    }

    private static (FieldInfo, RequiredAttribute)[] GetEntries(Type entryType)
    {
        if (Cache.TryGetValue(entryType, out var cached))
        {
            return cached;
        }

        List<(FieldInfo, RequiredAttribute)> entries = new();

        // Walk the hierarchy so private fields on base classes are picked up too.
        for (Type type = entryType; type != null && type != typeof(MonoBehaviour); type = type.BaseType)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (FieldInfo field in fields)
            {
                RequiredAttribute attribute = field.GetCustomAttribute<RequiredAttribute>();

                if (attribute != null)
                {
                    entries.Add((field, attribute));
                }
            }
        }

        var array = entries.ToArray();
        Cache[entryType] = array;

        return array;
    }
}