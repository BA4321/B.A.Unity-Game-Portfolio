using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile2D : MonoBehaviour
{
    [Header("Behaviour")]
    [SerializeField] private bool rotateToMoveDirection = true;
    [SerializeField] private bool returnToPoolOnPlayerHit = true;
    [SerializeField] private bool returnToPoolOnNonPlayerCollision = true;

    private Rigidbody2D rb;
    private Collider2D projectileCollider;

    private readonly List<Collider2D> ignoredOwnerColliders = new List<Collider2D>();

    private float damage;
    private GameObject owner;
    private int playerLayer;
    private bool activeProjectile;
    private Action<EnemyProjectile2D> releaseToPool;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();
        playerLayer = LayerMask.NameToLayer("Player");
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    public void Launch(
        Vector2 spawnPosition,
        Vector2 targetPosition,
        float speed,
        float damageAmount,
        float lifetime,
        GameObject ownerObject,
        Action<EnemyProjectile2D> releaseToPool)
    {
        this.damage = damageAmount;
        this.owner = ownerObject;
        this.releaseToPool = releaseToPool;
        activeProjectile = true;

        ClearIgnoredOwnerCollisions();

        rb.position = spawnPosition;
        rb.rotation = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Vector2 direction = (targetPosition - spawnPosition).normalized;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        if (rotateToMoveDirection)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }

        IgnoreOwnerCollisions();

        rb.linearVelocity = direction * speed;

        CancelInvoke();

        if (lifetime > 0f)
            Invoke(nameof(ReturnToPool), lifetime);
    }

    public void ResetForPool()
    {
        activeProjectile = false;

        CancelInvoke();

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        ClearIgnoredOwnerCollisions();

        owner = null;
        releaseToPool = null;
        damage = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider);
    }

    private void HandleHit(Collider2D other)
    {
        if (!activeProjectile || other == null)
            return;

        if (owner != null)
        {
            if (other.gameObject == owner || other.transform.IsChildOf(owner.transform))
                return;
        }

        if (playerLayer == -1)
        {
            Debug.LogWarning("Layer named 'Player' does not exist.");
            ReturnToPool();
            return;
        }

        if (other.gameObject.layer == playerLayer)
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();

            if (damageable != null)
                damageable.TakeDamage(damage);

            if (returnToPoolOnPlayerHit)
                ReturnToPool();

            return;
        }

        if (returnToPoolOnNonPlayerCollision)
            ReturnToPool();
    }

    private void IgnoreOwnerCollisions()
    {
        if (owner == null || projectileCollider == null)
            return;

        Collider2D[] ownerColliders = owner.GetComponentsInChildren<Collider2D>(true);

        for (int i = 0; i < ownerColliders.Length; i++)
        {
            Collider2D ownerCollider = ownerColliders[i];

            if (ownerCollider == null)
                continue;

            Physics2D.IgnoreCollision(projectileCollider, ownerCollider, true);
            ignoredOwnerColliders.Add(ownerCollider);
        }
    }

    private void ClearIgnoredOwnerCollisions()
    {
        if (projectileCollider == null)
        {
            ignoredOwnerColliders.Clear();
            return;
        }

        for (int i = 0; i < ignoredOwnerColliders.Count; i++)
        {
            Collider2D ownerCollider = ignoredOwnerColliders[i];

            if (ownerCollider != null)
                Physics2D.IgnoreCollision(projectileCollider, ownerCollider, false);
        }

        ignoredOwnerColliders.Clear();
    }

    private void ReturnToPool()
    {
        if (!activeProjectile)
            return;

        activeProjectile = false;
        releaseToPool?.Invoke(this);
    }
}