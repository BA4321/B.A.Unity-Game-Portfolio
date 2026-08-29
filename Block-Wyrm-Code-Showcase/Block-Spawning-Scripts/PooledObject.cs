using System.Collections.Generic;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public SharedObjectPool OwnerPool { get; private set; }
    public GameObject PrefabKey { get; private set; }
    public bool IsInPool { get; set; }

    private IPoolable[] poolableCallbacks;

    public void Initialize(SharedObjectPool ownerPool, GameObject prefabKey)
    {
        OwnerPool = ownerPool;
        PrefabKey = prefabKey;
        CacheCallbacks();
    }

    private void CacheCallbacks()
    {
        var allBehaviours = GetComponentsInChildren<MonoBehaviour>(true);
        List<IPoolable> found = new List<IPoolable>();

        foreach (var behaviour in allBehaviours)
        {
            if (behaviour is IPoolable poolable)
                found.Add(poolable);
        }

        poolableCallbacks = found.ToArray();
    }

    public void NotifyTakenFromPool()
    {
        if (poolableCallbacks == null)
            return;

        for (int i = 0; i < poolableCallbacks.Length; i++)
            poolableCallbacks[i].OnTakenFromPool();
    }

    public void NotifyReturnedFromPool()
    {
        if (poolableCallbacks == null)
            return;

        for (int i = 0; i < poolableCallbacks.Length; i++)
            poolableCallbacks[i].OnReturnedToPool();
    }

    public void ReturnToPool()
    {
        if (OwnerPool != null)
            OwnerPool.Return(gameObject);
        else
            Destroy(gameObject);
    }
}