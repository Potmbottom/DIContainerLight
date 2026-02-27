using System.Linq;
using UnityEngine;

public class PoolGameObject : MonoBehaviour, IPoolObject
{
    private IPoolManager _pool;
    private IUnbindableControl[] _allUnbindableControls;
    private PoolGameObject[] _childPoolObjects;
    
    private Vector3 _initialPosition;
    private Vector2 _initialAnchorMin;
    private Vector2 _initialAnchorMax;
    private Vector2 _initialPivot;
    
    private void Awake()
    {
        _allUnbindableControls = GetComponentsInChildren<IUnbindableControl>(true);
        _childPoolObjects = GetComponentsInChildren<PoolGameObject>(true)
            .Where(o => o != this)
            .ToArray();
    }

    public void OnSpawned(IPoolManager pool)
    {
        _pool = pool;
    }
    
    public void OnDespawned()
    {
        _pool = null;
    }

    public virtual void Release()
    {
        if (_pool == null) return;
        if (_childPoolObjects != null)
        {
            for (var i = 0; i < _childPoolObjects.Length; i++)
            {
                var child = _childPoolObjects[i];
                if (child != null)
                {
                    child.Release();
                }
            }
        }
        
        if (_allUnbindableControls != null)
        {
            for (var i = 0; i < _allUnbindableControls.Length; i++)
            {
                var control = _allUnbindableControls[i];
                if (control != null)
                {
                    control.Unbind();
                }
            }
        }
        
        _pool.Release(this);
    }
}