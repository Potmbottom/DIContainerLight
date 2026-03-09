using System;
using System.Collections.Generic;

public class DiContext : IDisposable, IBinder, IResolver, IPoolHandler
{
    public List<BindingModel> Bindings { get; } = new List<BindingModel>();
    public List<BindingConstructionModel> ConstructionData { get; } = new List<BindingConstructionModel>();
     
    private readonly DependencyResolver _resolver;
    private readonly DependencyBuilder _builder;
     
    private readonly List<IPoolManager> _pools = new List<IPoolManager>();

    public DiContext(IInjectionStrategy injectionStrategy)
    {
        _resolver = new DependencyResolver(injectionStrategy);
        _builder = new DependencyBuilder(injectionStrategy);
    }

    public void AddToBind(BindingConstructionModel constructionModel)
    {
        ConstructionData.Add(constructionModel);
    }
     
    public void ResolveDependencies()
    {
        var toResolve = _builder.Build(ConstructionData, Bindings);
        foreach (var obj in toResolve)
        {
            _resolver.Resolve(obj, Bindings);
        }
        ConstructionData.Clear();
        _pools.ForEach(manager => manager.Init());
    }
     
    public void Resolve(object obj)
    {
        _resolver.Resolve(obj, Bindings);
    }

    public void BindPool(IPoolManager pool)
    {
        _pools.Add(pool);
    }

    public void InitPools()
    {
        _pools.ForEach(manager => manager.Init());
    }

    public void DisposePools()
    {
        _pools.ForEach(manager => manager.Dispose());
    }

    public void Dispose()
    {
        var disposed = new HashSet<object>();
        foreach (var item in Bindings)
        {
            if (item.Object is IDisposable disposable && disposed.Add(item.Object))
            {
                disposable.Dispose();
            }
        }
        
        DisposePools();
        Bindings.Clear();
    }
}