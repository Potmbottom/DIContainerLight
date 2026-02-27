using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DependencyBuilder
{
    private const int CIRCULAR_DEPENDENCY_ERROR = -1;
    
    private readonly List<Type> _ignoreTypes = new List<Type> {typeof(List<>), typeof(Dictionary<,>)};
    private readonly IInjectionStrategy _injectionStrategy;

    public DependencyBuilder(IInjectionStrategy injectionStrategy)
    {
        _injectionStrategy = injectionStrategy;
    }

    public IEnumerable<object> Build(List<BindingConstructionModel> constructionData, List<BindingModel> bindings)
    {
        var needResolve = new List<object>();
        var ordered = GetOrderedBindings(constructionData);
        foreach (var binding in ordered)
        {
            var obj = binding.Getter.Build();
            binding.Aggregate?.Invoke(obj);
            
            var toBind = new List<Type> {binding.BindingType};
            if (binding.Interfaces != null)
                toBind.AddRange(binding.Interfaces);
            bindings.AddRange(toBind.Select(type => new BindingModel(type, binding.Contract, obj)));

            if (!IsNeedResolve(binding)) continue;
            needResolve.Add(obj);
        }

        bool IsNeedResolve(BindingConstructionModel model)
        {
            bool IsIgnored()
            {
                if (model.BindingType.IsPrimitive) return true;
                if (model.BindingType.IsGenericType &&
                    _ignoreTypes.Contains(model.BindingType.GetGenericTypeDefinition())) return true;
                return false;
            }

            bool IsAlreadyResolved()
            {
                return model.Getter.GetType().GetGenericTypeDefinition() == typeof(InstanceObjectBuilder<>);
            }

            return !IsIgnored() && !IsAlreadyResolved();
        }

        return needResolve;
    }

    private IEnumerable<BindingConstructionModel> GetOrderedBindings(IEnumerable<BindingConstructionModel> bindings)
    {
        var dict = new Dictionary<BindingConstructionModel, int>();
        foreach (var model in bindings)
        {
            var methodDepth = model.BindingType.IsPrimitive ? 0 : CalculateDepth(model.BindingType, model.BindingType);

            if (methodDepth == CIRCULAR_DEPENDENCY_ERROR)
                break;

            dict.Add(model, methodDepth);
        }

        return dict.OrderBy(pair => pair.Value)
            .Select(dict1 => dict1.Key).ToList();
    }
    
    //Type root only for debug
    private int CalculateDepth(Type type, HashSet<Type> visited, int depth = 1)
    {
        if (type.IsPrimitive) return depth;
        
        if (!visited.Add(type)) 
        {
            return CircularDependencyError(type); 
        }

        var methods = _injectionStrategy.GetInjectionMethods(type);
        var fields = _injectionStrategy.GetInjectionFields(type);
    
        var maxDepth = depth;

        if (methods != null)
        {
            foreach (var method in methods)
            {
                foreach (var paramInfo in method.GetParameters())
                {
                    var branchVisited = new HashSet<Type>(visited);
                    var branchDepth = CalculateDepth(paramInfo.ParameterType, branchVisited, depth + 1);
                
                    if (branchDepth == CIRCULAR_DEPENDENCY_ERROR) return CIRCULAR_DEPENDENCY_ERROR;
                    if (branchDepth > maxDepth) maxDepth = branchDepth;
                }
            }
        }

        if (fields != null)
        {
            foreach (var field in fields)
            {
                var branchVisited = new HashSet<Type>(visited);
                var branchDepth = CalculateDepth(field.FieldType, branchVisited, depth + 1);
                
                if (branchDepth == CIRCULAR_DEPENDENCY_ERROR) return CIRCULAR_DEPENDENCY_ERROR;
                if (branchDepth > maxDepth) maxDepth = branchDepth;
            }
        }
        
        return depth;
    }

    private int CircularDependencyError(Type type)
    {
        Debug.LogError($"Find circular dependency. Loop detected at type: {type}");
        return CIRCULAR_DEPENDENCY_ERROR;
    }
}