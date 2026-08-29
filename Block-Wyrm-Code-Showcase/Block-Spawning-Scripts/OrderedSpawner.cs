using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class OrderedSpawner : MonoBehaviour, IPoolable
{
    [System.Serializable]
    public class SpawnItem
    {
        public GameObject prefab;
        public Transform spawnPoint;

        [Tooltip("Seconds to wait BEFORE this object appears (scaled by Time.timeScale)")]
        public float delay = 0f;

        public Vector3 eulerRotation;
    }

    [Header("Spawn plan (order = spawn order)")]
    [SerializeField] private List<SpawnItem> spawnPlan = new List<SpawnItem>();

    [Header("Timing")]
    [Min(0f)]
    [SerializeField] private float timeMultiplier = 1f;

    [Header("Start Behaviour")]
    [Tooltip("If true, this OrderedSpawner starts by itself in Start(). Turn this off for manager-controlled spawners.")]
    [SerializeField] private bool autoBeginOnStart = true;

    [Header("Looping")]
    [Tooltip("If true, repeat the whole plan after it finishes.")]
    [SerializeField] private bool loopAfterFinish = true;

    [Header("Universal Theme For All Spawned Blocks")]
    [SerializeField] private BlockTheme theme = BlockTheme.Set1;

    public event Action OnCycleFinished;

    public bool CompletedOnce { get; private set; } = false;
    public bool IsRunning => runRoutine != null;

    private Coroutine runRoutine;

    private void Start()
    {
        if (autoBeginOnStart)
            BeginSpawning();
    }

    public void BeginSpawning()
    {
        if (!isActiveAndEnabled)
            return;

        if (runRoutine != null)
            return;

        if (spawnPlan == null || spawnPlan.Count == 0)
            return;

        CompletedOnce = false;
        runRoutine = StartCoroutine(RunLoop());
    }

    public void ConfigureForExternalControl(BlockTheme newTheme, bool shouldLoop)
    {
        autoBeginOnStart = false;

        StopRunning();

        theme = newTheme;
        loopAfterFinish = shouldLoop;
        CompletedOnce = false;
    }

    public void SetTheme(BlockTheme newTheme)
    {
        theme = newTheme;
    }

    public BlockTheme GetTheme()
    {
        return theme;
    }

    public void SetLooping(bool shouldLoop)
    {
        loopAfterFinish = shouldLoop;
    }

    public void StopAfterThisCycle()
    {
        loopAfterFinish = false;
    }

    private IEnumerator RunLoop()
    {
        do
        {
            yield return RunOneCycle();

            CompletedOnce = true;
            OnCycleFinished?.Invoke();

        } while (loopAfterFinish);

        runRoutine = null;
    }

    private IEnumerator RunOneCycle()
    {
        foreach (var item in spawnPlan)
        {
            if (item == null || item.prefab == null || item.spawnPoint == null)
                continue;

            if (item.spawnPoint.parent != transform)
            {
                Debug.LogWarning($"{name}: '{item.spawnPoint.name}' is not a child of this spawner – skipped.");
                continue;
            }

            if (timeMultiplier > 0f && item.delay > 0f)
                yield return new WaitForSeconds(item.delay * timeMultiplier);

            Quaternion rot = Quaternion.Euler(item.eulerRotation);
            GameObject spawned = null;

            if (SharedObjectPool.Instance != null)
            {
                spawned = SharedObjectPool.Instance.Spawn(
                    item.prefab,
                    item.spawnPoint.position,
                    rot
                );
            }
            else
            {
                spawned = Instantiate(
                    item.prefab,
                    item.spawnPoint.position,
                    rot
                );
            }

            if (spawned != null)
            {
                ThemedSpriteSet themed = spawned.GetComponent<ThemedSpriteSet>();
                if (themed != null)
                    themed.ApplyTheme(theme);
            }
        }
    }

    private void StopRunning()
    {
        if (runRoutine != null)
        {
            StopCoroutine(runRoutine);
            runRoutine = null;
        }
    }

    public void OnTakenFromPool()
    {
        CompletedOnce = false;
        StopRunning();
    }

    public void OnReturnedToPool()
    {
        CompletedOnce = false;
        StopRunning();
    }
}