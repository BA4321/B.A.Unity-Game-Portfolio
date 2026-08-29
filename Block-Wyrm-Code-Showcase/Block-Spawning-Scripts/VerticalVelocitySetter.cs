using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class VerticalVelocityFreezeY : MonoBehaviour, IPoolable
{
    [SerializeField] private float verticalSpeed = -4f;
    [SerializeField] private bool enableSpawnerOnFinish = false;

    private Rigidbody2D rb;
    private float spawnTime;
    private bool finished;

    private RigidbodyConstraints2D initialConstraints;
    private OrderedSpawner cachedSpawner;
    private bool initialSpawnerEnabled;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialConstraints = rb.constraints;

        cachedSpawner = GetComponent<OrderedSpawner>();
        if (cachedSpawner != null)
            initialSpawnerEnabled = cachedSpawner.enabled;

        ResetForFreshSpawn();
    }

    private void FixedUpdate()
    {
        if (finished) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalSpeed);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (finished || Time.time - spawnTime < 0.5f)
            return;

        if (col.gameObject.layer == LayerMask.NameToLayer("ground"))
        {
            StopMovement();
        }
    }

    private void StopMovement()
    {
        finished = true;
        rb.linearVelocity = Vector2.zero;

        rb.constraints = initialConstraints | RigidbodyConstraints2D.FreezePositionY;

        if (enableSpawnerOnFinish && cachedSpawner != null)
        {
            cachedSpawner.enabled = true;
            cachedSpawner.BeginSpawning();
        }

        enabled = false;
    }

    private void ResetForFreshSpawn()
    {
        finished = false;
        enabled = true;
        spawnTime = Time.time;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = initialConstraints;

        if (cachedSpawner != null)
            cachedSpawner.enabled = initialSpawnerEnabled;
    }

    public void OnTakenFromPool()
    {
        ResetForFreshSpawn();
    }

    public void OnReturnedToPool()
    {
        finished = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = initialConstraints;
        }

        if (cachedSpawner != null)
            cachedSpawner.enabled = initialSpawnerEnabled;
    }
}