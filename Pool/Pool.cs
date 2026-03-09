using System;
using System.Collections.Generic;

public class Pool : IPool
{
    private readonly Queue<object> _pool = new Queue<object>();
    private readonly Func<object> _getFunc;

    public int Count => _pool.Count;
    
    public Pool(Func<object> getFunc)
    {
        _getFunc = getFunc;
    }
    
    public object Get()
    {
        if (_pool.Count == 0)
            throw new InvalidOperationException(
                $"Pool is empty. Call Expand() before Get(), or use PoolManager which handles this automatically.");
        
        return _pool.Dequeue();
    }
    
    public virtual void Release(object data)
    {
        _pool.Enqueue(data);
    }

    public void Expand(int count, Action<object> onElementCreate)
    {
        for (var i = 0; i < count; i++)
        {
            var item = _getFunc.Invoke();
            _pool.Enqueue(item);
            onElementCreate.Invoke(item);
        }
    }
}