using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyRangedAttack2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private EnemyProjectile2D projectilePrefab;

    [Header("Projectile Settings")]
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private float projectileLifetime = 5f;

    [Header("Pooling")]
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxPoolSize = 30;
    [SerializeField] private int prewarmCount = 10;
    [SerializeField] private bool collectionCheck = true;

    private ObjectPool<EnemyProjectile2D> projectilePool;

    private void Awake()
    {
        CreatePool();
        PrewarmPool();
    }

    private void OnDestroy()
    {
        projectilePool?.Clear();
    }

    public void FireAtPlayer()
    {
        if (firePoint == null)
        {
            Debug.LogWarning($"{name}: EnemyRangedAttack2D is missing firePoint.", this);
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{name}: EnemyRangedAttack2D is missing projectilePrefab.", this);
            return;
        }

        if (PlayerHealthManager.Instance == null)
        {
            Debug.LogWarning($"{name}: PlayerHealthManager.Instance is null.", this);
            return;
        }

        EnemyProjectile2D projectile = projectilePool.Get();

        projectile.Launch(
            spawnPosition: firePoint.position,
            targetPosition: PlayerHealthManager.Instance.transform.position,
            speed: projectileSpeed,
            damageAmount: projectileDamage,
            lifetime: projectileLifetime,
            ownerObject: gameObject,
            releaseToPool: ReleaseProjectile
        );
    }


    public void SetProjectileDamage(float newDamage)
    {
       projectileDamage = Mathf.Max(0f, newDamage);
    }

    private void CreatePool()
    {
        projectilePool = new ObjectPool<EnemyProjectile2D>(
            createFunc: CreateProjectile,
            actionOnGet: OnGetProjectile,
            actionOnRelease: OnReleaseProjectile,
            actionOnDestroy: OnDestroyProjectile,
            collectionCheck: collectionCheck,
            defaultCapacity: Mathf.Max(1, defaultCapacity),
            maxSize: Mathf.Max(1, maxPoolSize)
        );
    }

    private EnemyProjectile2D CreateProjectile()
    {
        EnemyProjectile2D projectile = Instantiate(projectilePrefab);
        projectile.gameObject.SetActive(false);
        return projectile;
    }

    private void OnGetProjectile(EnemyProjectile2D projectile)
    {
        projectile.gameObject.SetActive(true);
    }

    private void OnReleaseProjectile(EnemyProjectile2D projectile)
    {
        projectile.ResetForPool();
        projectile.gameObject.SetActive(false);
    }

    private void OnDestroyProjectile(EnemyProjectile2D projectile)
    {
        if (projectile != null)
            Destroy(projectile.gameObject);
    }

    private void ReleaseProjectile(EnemyProjectile2D projectile)
    {
        if (projectile != null)
            projectilePool.Release(projectile);
    }

    private void PrewarmPool()
    {
        if (prewarmCount <= 0)
            return;

        List<EnemyProjectile2D> temp = new List<EnemyProjectile2D>(prewarmCount);

        for (int i = 0; i < prewarmCount; i++)
            temp.Add(projectilePool.Get());

        for (int i = 0; i < temp.Count; i++)
            projectilePool.Release(temp[i]);
    }
}