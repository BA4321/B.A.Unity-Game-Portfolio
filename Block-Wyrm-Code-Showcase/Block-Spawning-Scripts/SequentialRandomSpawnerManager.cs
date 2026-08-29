using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drops one random spawner prefab at a time and waits for it to finish a single cycle.
/// Each prefab is used once per sequence.
/// If loopAfterAllSpawnersUsed is enabled, the sequence refills and starts again forever.
/// Spawn position/rotation are LOCAL to instancesParent if set, otherwise world-space.
/// </summary>
public class SequentialRandomSpawnerManager : MonoBehaviour
{
    [Header("Spawner Prefabs (each must contain an OrderedSpawner)")]
    [Tooltip("Each element is a prefab that has an OrderedSpawner inside it.")]
    [SerializeField] private List<GameObject> spawnerPrefabs = new List<GameObject>();

    [Header("Universal Theme For All Spawned OrderedSpawners")]
    [SerializeField] private BlockTheme universalTheme = BlockTheme.Set1;

    [Header("Instantiate Options")]
    [Tooltip("Optional parent for spawned spawner instances. Keeps Hierarchy tidy.")]
    [SerializeField] private Transform instancesParent;

    [Tooltip("Spawn position relative to instancesParent. If no parent, this is world-space.")]
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;

    [Tooltip("Spawn rotation relative to instancesParent. If no parent, this is world-space.")]
    [SerializeField] private Vector3 spawnEulerRotation = Vector3.zero;

    [Header("Cleanup")]
    [Tooltip("Destroy each spawner GameObject after it completes its cycle.")]
    [SerializeField] private bool destroySpawnerInstanceOnFinish = true;

    [Header("Looping")]
    [Tooltip("If enabled, after every spawner prefab has spawned once, the manager refills the list and starts again.")]
    [SerializeField] private bool loopAfterAllSpawnersUsed = false;

    [Header("Timing")]
    [Tooltip("Delay in seconds between finishing one spawner and starting the next.")]
    [SerializeField, Min(0f)] private float interSpawnerDelaySeconds = 0f;

    [Header("Enemy Gate")]
    [Tooltip("Set this to your Enemy layer, or a mask that includes it.")]
    [SerializeField] private LayerMask enemyLayerMask;

    [Tooltip("If enabled, the manager will not start the next spawner until there are 0 objects in enemyLayerMask.")]
    [SerializeField] private bool waitForNoEnemiesBeforeNextSpawner = true;

    [Header("Final Event")]
    [Tooltip("This prefab will be spawned after all spawners are finished and the final enemies are dead. This only happens if looping is disabled.")]
    [SerializeField] private GameObject specialFinalPrefab;

    private readonly List<int> _unusedIndexes = new List<int>();

    private void Start()
    {
        if (spawnerPrefabs == null || spawnerPrefabs.Count == 0)
        {
            Debug.LogWarning($"{name}: No spawner prefabs assigned.");
            return;
        }

        StartCoroutine(RunSpawnerSequence());
    }

    public void SetUniversalTheme(BlockTheme newTheme)
    {
        universalTheme = newTheme;
    }

    public BlockTheme GetUniversalTheme()
    {
        return universalTheme;
    }

    private IEnumerator RunSpawnerSequence()
    {
        while (true)
        {
            RefillUnusedIndexes();

            if (_unusedIndexes.Count == 0)
            {
                Debug.LogWarning($"{name}: No valid spawner prefabs assigned.");
                yield break;
            }

            while (_unusedIndexes.Count > 0)
            {
                int pick = Random.Range(0, _unusedIndexes.Count);
                int prefabIndex = _unusedIndexes[pick];
                _unusedIndexes.RemoveAt(pick);

                GameObject prefab = spawnerPrefabs[prefabIndex];
                if (prefab == null)
                    continue;

                GameObject instance = SpawnObject(prefab);

                yield return StartCoroutine(HandleSpawnerCycle(instance, prefab.name));

                if (_unusedIndexes.Count > 0)
                {
                    yield return WaitDelayThenUntilNoEnemies();
                }
            }

            // Full sequence finished.
            // Wait before either restarting or spawning the final prefab.
            yield return WaitDelayThenUntilNoEnemies();

            if (!loopAfterAllSpawnersUsed)
            {
                if (specialFinalPrefab != null)
                {
                    Debug.Log($"{name}: All waves complete. Spawning special final prefab.");
                    SpawnObject(specialFinalPrefab);
                }

                yield break;
            }

            Debug.Log($"{name}: All spawner prefabs were used once. Looping sequence again.");
        }
    }

    private void RefillUnusedIndexes()
    {
        _unusedIndexes.Clear();

        for (int i = 0; i < spawnerPrefabs.Count; i++)
        {
            if (spawnerPrefabs[i] != null)
                _unusedIndexes.Add(i);
        }
    }

    private IEnumerator HandleSpawnerCycle(GameObject instance, string prefabName)
{
    OrderedSpawner spawner = instance.GetComponentInChildren<OrderedSpawner>(true);
    if (spawner == null)
    {
        Debug.LogWarning($"{name}: The selected prefab '{prefabName}' has no OrderedSpawner. Skipping.");

        if (destroySpawnerInstanceOnFinish && instance != null)
            Destroy(instance);

        yield break;
    }

    bool finished = false;

    void Handler()
    {
        finished = true;
    }

    // Manager fully takes ownership before spawning starts.
    spawner.ConfigureForExternalControl(
        universalTheme,
        shouldLoop: false
    );

    // Subscribe before BeginSpawning, so the event cannot be missed.
    spawner.OnCycleFinished += Handler;

    spawner.BeginSpawning();

    while (spawner != null && !finished && !spawner.CompletedOnce)
        yield return null;

    if (spawner != null)
        spawner.OnCycleFinished -= Handler;

    if (destroySpawnerInstanceOnFinish && instance != null)
        Destroy(instance);
}
    private GameObject SpawnObject(GameObject prefabToSpawn)
    {
        GameObject newObj;

        if (instancesParent != null)
        {
            newObj = Instantiate(prefabToSpawn, instancesParent, false);
            newObj.transform.localPosition = spawnPosition;
            newObj.transform.localRotation = Quaternion.Euler(spawnEulerRotation);
        }
        else
        {
            Quaternion worldRot = Quaternion.Euler(spawnEulerRotation);
            newObj = Instantiate(prefabToSpawn, spawnPosition, worldRot);
        }

        return newObj;
    }

    private IEnumerator WaitDelayThenUntilNoEnemies()
    {
        if (interSpawnerDelaySeconds > 0f)
            yield return new WaitForSeconds(interSpawnerDelaySeconds);

        if (!waitForNoEnemiesBeforeNextSpawner)
            yield break;

        if (enemyLayerMask.value == 0)
            yield break;

        while (CountObjectsInLayerMask(enemyLayerMask) > 0)
        {
            if (interSpawnerDelaySeconds > 0f)
                yield return new WaitForSeconds(interSpawnerDelaySeconds);
            else
                yield return null;
        }
    }

    private static int CountObjectsInLayerMask(LayerMask mask)
    {
        int m = mask.value;
        int count = 0;

        var all = GameObject.FindObjectsByType<GameObject>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        for (int i = 0; i < all.Length; i++)
        {
            GameObject go = all[i];
            if (go == null)
                continue;

            if ((m & (1 << go.layer)) != 0)
                count++;
        }

        return count;
    }
}