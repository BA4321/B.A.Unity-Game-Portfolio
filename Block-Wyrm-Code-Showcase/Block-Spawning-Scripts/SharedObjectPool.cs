using System.Collections.Generic;
using UnityEngine;

public interface IPoolable
{
    void OnTakenFromPool();
    void OnReturnedToPool();
}

public class SharedObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolEntry
    {
        public GameObject prefab;
        [Min(0)] public int prewarmCount = 10;
        public bool canExpand = true;
    }

    public static SharedObjectPool Instance { get; private set; }

    [Header("Pool Setup")]
    [SerializeField] private List<PoolEntry> entries = new List<PoolEntry>();
    [SerializeField] private Transform pooledObjectsParent;

    private readonly Dictionary<GameObject, Queue<PooledObject>> availableByPrefab = new();
    private readonly Dictionary<GameObject, PoolEntry> entryByPrefab = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (pooledObjectsParent == null)
            pooledObjectsParent = transform;

        BuildPools();
    }

    private void BuildPools()
    {
        availableByPrefab.Clear();
        entryByPrefab.Clear();

        foreach (var entry in entries)
        {
            if (entry == null || entry.prefab == null)
                continue;

            if (entryByPrefab.ContainsKey(entry.prefab))
            {
                Debug.LogWarning($"SharedObjectPool: Duplicate pool entry found for prefab '{entry.prefab.name}'. Skipping duplicate.");
                continue;
            }

            entryByPrefab.Add(entry.prefab, entry);
            availableByPrefab.Add(entry.prefab, new Queue<PooledObject>());

            for (int i = 0; i < entry.prewarmCount; i++)
            {
                var pooled = CreateNewInstance(entry.prefab);
                ReturnInstanceToQueue(pooled);
            }
        }
    }

    private PooledObject CreateNewInstance(GameObject prefab)
    {
        GameObject go = Instantiate(prefab, pooledObjectsParent);
        go.SetActive(false);

        PooledObject pooled = go.GetComponent<PooledObject>();
        if (pooled == null)
            pooled = go.AddComponent<PooledObject>();

        pooled.Initialize(this, prefab);
        return pooled;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
            return null;

        if (!availableByPrefab.TryGetValue(prefab, out var queue))
        {
            Debug.LogWarning($"SharedObjectPool: No pool entry exists for prefab '{prefab.name}'.");
            return null;
        }

        PooledObject pooled = null;

        while (queue.Count > 0 && pooled == null)
            pooled = queue.Dequeue();

        if (pooled == null)
        {
            if (!entryByPrefab[prefab].canExpand)
            {
                Debug.LogWarning($"SharedObjectPool: Pool exhausted for '{prefab.name}' and expansion is disabled.");
                return null;
            }

            pooled = CreateNewInstance(prefab);
        }

        pooled.IsInPool = false;

        Transform tr = pooled.transform;
        tr.SetParent(null);
        tr.SetPositionAndRotation(position, rotation);

        pooled.NotifyTakenFromPool();
        pooled.gameObject.SetActive(true);

        return pooled.gameObject;
    }

    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        if (prefab == null)
            return null;

        GameObject go = Spawn(prefab.gameObject, position, rotation);
        if (go == null)
            return null;

        return go.GetComponent<T>();
    }

    public void Return(GameObject instance)
    {
        if (instance == null)
            return;

        PooledObject pooled = instance.GetComponent<PooledObject>();
        if (pooled == null || pooled.OwnerPool != this)
        {
            Destroy(instance);
            return;
        }

        if (pooled.IsInPool)
            return;

        ReturnInstanceToQueue(pooled);
    }

    private void ReturnInstanceToQueue(PooledObject pooled)
    {
        if (pooled == null)
            return;

        pooled.NotifyReturnedFromPool();

        pooled.IsInPool = true;
        pooled.transform.SetParent(pooledObjectsParent);
        pooled.gameObject.SetActive(false);

        if (!availableByPrefab.TryGetValue(pooled.PrefabKey, out var queue))
        {
            Debug.LogWarning($"SharedObjectPool: Missing queue for prefab key '{pooled.PrefabKey.name}'. Destroying instance.");
            Destroy(pooled.gameObject);
            return;
        }

        queue.Enqueue(pooled);
    }
}