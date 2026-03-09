using UnityEngine;

public class FactoryFromPrefab<T> : IFactory<T>
{
    private readonly string _loadPath;
    private GameObject _cachedPrefab;

    public FactoryFromPrefab(string path)
    {
        _loadPath = path;
    }

    public virtual T Create()
    {
        if (_cachedPrefab == null)
        {
            _cachedPrefab = Resources.Load<GameObject>(_loadPath);
            if (_cachedPrefab == null)
            {
                Debug.LogError($"FactoryFromPrefab: Failed to load prefab at Resources/{_loadPath}");
                return default;
            }
        }
        
        var instance = Object.Instantiate(_cachedPrefab);
        return instance.GetComponent<T>();
    }
}