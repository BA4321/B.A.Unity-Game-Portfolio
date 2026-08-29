using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class Bullet2D_Mobile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float lifeSeconds = 3f;

    [Header("Filtering")]
    [Tooltip("Set this to include the 'Enemy' layer. Include 'World' if you want bullets to break on walls.")]
    [SerializeField] private LayerMask hitMask = ~0;

    private float _damage;
    private DamageType _damageType = DamageType.Normal;
    private GameObject _owner;
    private bool _initialized;

    // Explosion Data
    private bool _hasExplosion;
    private float _explosionRadius;

    // Explosion VFX Pool Data
    private ExplosionVfxPool2D _explosionVfxPool;
    private float _explosionVfxScalePerRadius = 1f;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    // Old Init kept for safety.
    public void Init(Vector2 dir, float speed, float damage, GameObject owner, bool hasExplosion = false, float explosionRadius = 0f)
    {
        Init(dir, speed, damage, owner, DamageType.Normal, hasExplosion, explosionRadius);
    }

    // Current Init kept for safety.
    public void Init(
        Vector2 dir,
        float speed,
        float damage,
        GameObject owner,
        DamageType damageType,
        bool hasExplosion = false,
        float explosionRadius = 0f)
    {
        Init(
            dir,
            speed,
            damage,
            owner,
            damageType,
            hasExplosion,
            explosionRadius,
            null,
            1f
        );
    }

    // New Init with pooled explosion VFX.
    public void Init(
        Vector2 dir,
        float speed,
        float damage,
        GameObject owner,
        DamageType damageType,
        bool hasExplosion,
        float explosionRadius,
        ExplosionVfxPool2D explosionVfxPool,
        float explosionVfxScalePerRadius)
    {
        _initialized = true;
        _damage = Mathf.Max(0f, damage);
        _damageType = damageType;
        _owner = owner;

        _hasExplosion = hasExplosion;
        _explosionRadius = Mathf.Max(0f, explosionRadius);

        _explosionVfxPool = explosionVfxPool;
        _explosionVfxScalePerRadius = Mathf.Max(0.01f, explosionVfxScalePerRadius);

        if (rb != null)
        {
            rb.linearVelocity = dir.normalized * speed;

            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rb.rotation = ang;
        }

        Destroy(gameObject, lifeSeconds);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_initialized) return;
        if (_owner != null && other.gameObject == _owner) return;

        if (((1 << other.gameObject.layer) & hitMask) == 0)
            return;

        if (_hasExplosion)
        {
            Explode();
        }
        else
        {
            DamageTarget(other.gameObject);
        }

        Destroy(gameObject);
    }

    private void Explode()
    {
        SpawnExplosionVfx();

        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, _explosionRadius, hitMask);

        HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();

        foreach (Collider2D col in objectsInRange)
        {
            EnemyHealth enemy = col.GetComponentInParent<EnemyHealth>();

            if (enemy != null && !hitEnemies.Contains(enemy))
            {
                enemy.TakeDamage(_damage, _damageType);
                hitEnemies.Add(enemy);
            }
        }
    }

    private void SpawnExplosionVfx()
    {
        if (_explosionVfxPool == null) return;
        if (_explosionRadius <= 0f) return;

        float finalScale = _explosionRadius * _explosionVfxScalePerRadius;
        _explosionVfxPool.Play(transform.position, finalScale);
    }

    private void DamageTarget(GameObject obj)
    {
        EnemyHealth enemy = obj.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            enemy.TakeDamage(_damage, _damageType);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_hasExplosion)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(transform.position, _explosionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}