using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class DefaultInjectionStrategy : IInjectionStrategy
{
    private readonly Dictionary<Type, MethodBase[]> _methodsCache = new Dictionary<Type, MethodBase[]>();
    private readonly Dictionary<Type, FieldInfo[]> _fieldsCache = new Dictionary<Type, FieldInfo[]>();

    public IEnumerable<MethodBase> GetInjectionMethods(Type type)
    {
        if (_methodsCache.TryGetValue(type, out var cachedMethods))
        {
            return cachedMethods;
        }

        IEnumerable<MethodBase> methodsToCache;

        if (IsUnityObject(type))
        {
            methodsToCache = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(info => info.GetCustomAttribute<InjectAttribute>() != null);
        }
        else
        {
            var ctor = (MethodBase)type.GetConstructors()
                .FirstOrDefault(info => info.GetParameters().Length > 0);
            methodsToCache = ctor == null ? Array.Empty<MethodBase>() : new[] { ctor };
        }
        
        var resultArray = methodsToCache.ToArray();
        _methodsCache[type] = resultArray;
            
        return resultArray;
    }

    public IEnumerable<FieldInfo> GetInjectionFields(Type type)
    {
        if (_fieldsCache.TryGetValue(type, out var cachedFields))
        {
            return cachedFields;
        }

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(info => info.GetCustomAttribute<InjectAttribute>() != null)
            .ToArray();

        _fieldsCache[type] = fields;
            
        return fields;
    }
    
    private bool IsUnityObject(Type parent)
    {
        while (parent != null && parent.BaseType != null)
        {
            parent = parent.BaseType;
            if (parent == typeof(MonoBehaviour))
                return true;
        }
        return false;
    }
}