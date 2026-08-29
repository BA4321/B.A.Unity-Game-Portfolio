using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KillOnGroundTouch2D : MonoBehaviour
{
    
    [Header("What counts as ground?")]
    [SerializeField] private LayerMask groundLayer; // Optional fast filter for trigger hit
    [SerializeField] private string groundTag = "Ground";

    [Header("How to find health")]
    [Tooltip("If left empty, will use GetComponentInParent<PlayerHealthManager>().")]
    [SerializeField] private PlayerHealthManager playerHealth;

    [Header("Player Controller")]
    [Tooltip("Used to prevent kill when the player is grounded.")]
    [SerializeField] private PlayerController2D_Mobile playerController;

    [Header("Player Collider References")]
    [Tooltip("Main collider used to confirm the player is currently touching Ground-tagged geometry.")]
    [SerializeField] private Collider2D mainPlayerCollider;

    [Header("Rescue / Forgiveness")]
    [Tooltip("How many times touching ground rescues instead of killing.")]
    [SerializeField] private int rescueUses = 2;

    [Tooltip("Child transforms attached to the player. EACH must have a Collider2D (usually trigger) used as a safety probe.")]
    [SerializeField] private Transform[] rescueSpots;

    [Tooltip("How long this script ignores further hits after a rescue.")]
    [SerializeField] private float disableDurationAfterRescue = 2f;

    [Tooltip("Reset player Rigidbody2D velocity when rescued (recommended).")]
    [SerializeField] private bool resetVelocityOnRescue = true;

    [Header("Wings effect, Enable and Disable")]
    private ScaleAndFadeEffect _effectScript;

    [Header("Fallback")]
    [Tooltip("If no rescue spot is safe, kill player immediately. If false, no rescue occurs and the script falls through to kill.")]
    [SerializeField] private bool killIfNoSafeRescueSpot = true;

    [Header("Debug")]
    [SerializeField] private bool logHit;
    [SerializeField] private bool logRescueChecks;

    private int rescuesUsed = 0;
    private bool temporarilyDisabled = false;

    // Reused buffer to avoid allocations in overlap checks
    private readonly Collider2D[] overlapBuffer = new Collider2D[32];
    
    private void Reset()
    {
        int ground = LayerMask.NameToLayer("Ground");
        if (ground >= 0) groundLayer = 1 << ground;

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Awake()
    {
        // Cache the reference to the script
        _effectScript = GetComponent<ScaleAndFadeEffect>();

        if (!playerHealth) playerHealth = GetComponentInParent<PlayerHealthManager>();
        if (!playerController) playerController = GetComponentInParent<PlayerController2D_Mobile>();

        if (!playerHealth)
            Debug.LogWarning("KillOnGroundTouch2D: No PlayerHealthManager found on parent hierarchy.", this);

        if (!mainPlayerCollider && playerHealth)
            mainPlayerCollider = playerHealth.GetComponent<Collider2D>();

        if (!mainPlayerCollider)
            mainPlayerCollider = GetComponentInParent<Collider2D>();

        if (!mainPlayerCollider)
            Debug.LogWarning("KillOnGroundTouch2D: No mainPlayerCollider assigned/found. Rescue spot checks may fail.", this);
    }

    private void OnTriggerEnter2D(Collider2D other) => TryKillOrRescue(other.gameObject);
    private void OnCollisionEnter2D(Collision2D collision) => TryKillOrRescue(collision.collider.gameObject);

    private void TryKillOrRescue(GameObject other)
{
    if (temporarilyDisabled) return;
    if (!playerHealth || !playerHealth.IsAlive) return;

    if (groundLayer.value != 0)
    {
        int mask = 1 << other.layer;
        if ((groundLayer.value & mask) == 0) return;
    }

    if (!string.IsNullOrEmpty(groundTag) && !other.CompareTag(groundTag))
        return;

    // Try rescue first (if uses remain)
    if (rescuesUsed < rescueUses)
    {
        // Only forgive grounded hits during the rescue phase

        if (TryFindSafeRescuePosition(out Vector3 targetPos))
        {
            ConsumeRescueUse();
            PerformRescue(targetPos);

            if (_effectScript) _effectScript.enabled = true;

            if (logHit)
                Debug.Log($"KillOnGroundTouch2D: RESCUE {rescuesUsed}/{rescueUses} -> teleported to safe spot.", this);

            return;
        }
        else if (logRescueChecks)
            Debug.Log("KillOnGroundTouch2D: No safe rescue spot found.", this);

        if (!killIfNoSafeRescueSpot)
            return;
    }

    // No rescues left -> kill, regardless of grounded state
    if (logHit)
        Debug.Log($"KillOnGroundTouch2D: touched ground '{other.name}', killing player.", this);

    playerHealth.TakeDamage(playerHealth.Current);
}

    private bool TryFindSafeRescuePosition(out Vector3 targetPosition)
    {
        targetPosition = Vector3.zero;

        // Confirm main collider is actually touching Ground-tagged object before rescue logic
        if (!mainPlayerCollider)
            return false;

        if (!IsTouchingGroundTaggedObject(mainPlayerCollider))
        {
            if (logRescueChecks)
                Debug.Log("KillOnGroundTouch2D: Main player collider is not touching Ground-tagged object, rescue skipped.", this);

            return false;
        }

        if (rescueSpots == null || rescueSpots.Length == 0)
            return false;

        for (int i = 0; i < rescueSpots.Length; i++)
        {
            Transform spot = rescueSpots[i];
            if (!spot) continue;

            Collider2D probe = spot.GetComponent<Collider2D>();
            if (!probe)
            {
                if (logRescueChecks)
                    Debug.LogWarning($"KillOnGroundTouch2D: Rescue spot '{spot.name}' has no Collider2D probe.", spot);
                continue;
            }

            bool blockedByGround = IsTouchingGroundTaggedObject(probe);

            if (logRescueChecks)
                Debug.Log($"Rescue spot [{i}] '{spot.name}' blockedByGround={blockedByGround}", spot);

            if (!blockedByGround)
            {
                // Teleport player root to this child transform's CURRENT world position
                targetPosition = spot.position;
                return true;
            }
        }

        return false;
    }

    private bool IsTouchingGroundTaggedObject(Collider2D probe)
    {
        if (!probe || !probe.enabled) return false;

        // Overlap check so this also works with trigger probe colliders
        ContactFilter2D filter = ContactFilter2D.noFilter;
        filter.useTriggers = true;


        int count = probe.Overlap(filter, overlapBuffer);
        Transform playerRoot = playerHealth ? playerHealth.transform.root : transform.root;

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = overlapBuffer[i];
            if (!hit) continue;

            // Ignore any collider on the player's own hierarchy
            if (hit.transform.root == playerRoot)
                continue;

            // Optional layer filter if set
            if (groundLayer.value != 0)
            {
                int mask = 1 << hit.gameObject.layer;
                if ((groundLayer.value & mask) == 0)
                    continue;
            }

            // Tag filter (requested behavior)
            if (!string.IsNullOrEmpty(groundTag) && !hit.CompareTag(groundTag))
                continue;

            return true;
        }

        return false;
    }

    private void PerformRescue(Vector3 targetPos)
    {
        Transform playerRoot = playerHealth ? playerHealth.transform : transform.root;

        // Disable immediately so teleport doesn't re-trigger this frame
        temporarilyDisabled = true;

        playerRoot.position = targetPos;

        if (resetVelocityOnRescue)
        {
            Rigidbody2D rb = playerRoot.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero; // compatible with older + current Unity versions
        }

        if (disableDurationAfterRescue > 0f)
            StartCoroutine(ReenableAfterDelay());
        else
            temporarilyDisabled = false;
    }

    private IEnumerator ReenableAfterDelay()
    {
        yield return new WaitForSeconds(disableDurationAfterRescue);
        temporarilyDisabled = false;
    }

    // Optional helpers
    public int RemainingRescues => Mathf.Max(0, rescueUses - rescuesUsed);
    public event Action<int> OnRemainingRescuesChanged;


    public void TriggerRescueFromButton()
    {
        // 1. Basic safety checks
        if (!playerHealth || !playerHealth.IsAlive) return;
        if (temporarilyDisabled) return;

        // 2. Enforce rescue limits
        if (rescuesUsed >= rescueUses)
        {
            Debug.Log("KillOnGroundTouch2D: Manual rescue failed. No rescues remaining.", this);
            return;
        }

        Vector3 targetPos = transform.position; // Fallback position
        bool foundSafeSpot = false;

        // 3. Find a safe spot without checking if the player is currently touching the ground
        if (rescueSpots != null && rescueSpots.Length > 0)
        {
            for (int i = 0; i < rescueSpots.Length; i++)
            {
                Transform spot = rescueSpots[i];
                if (!spot) continue;

                Collider2D probe = spot.GetComponent<Collider2D>();
                // Check if the spot itself is clear of the ground
                if (probe && !IsTouchingGroundTaggedObject(probe))
                {
                    targetPos = spot.position;
                    foundSafeSpot = true;
                    break;
                }
            }

            // Fallback: If ALL spots are blocked but you still want to force a teleport
            if (!foundSafeSpot && rescueSpots[0] != null)
            {
                targetPos = rescueSpots[0].position;
                Debug.LogWarning("KillOnGroundTouch2D: No safe spots found. Forcing teleport to first spot anyway.", this);
            }
        }
        else
        {
            Debug.LogWarning("KillOnGroundTouch2D: Cannot manual rescue. No rescue spots assigned!", this);
            return;
        }

        // 4. Consume a rescue use and execute
        ConsumeRescueUse();
        PerformRescue(targetPos);

        // 5. Trigger the visual effect
        if (_effectScript) _effectScript.enabled = true;

        Debug.Log($"KillOnGroundTouch2D: Manual rescue triggered via button. {rescuesUsed}/{rescueUses} used.", this);
    }
    private void ConsumeRescueUse()
    {
    SetRescuesUsed(rescuesUsed + 1);
    }

    private void SetRescuesUsed(int newValue)
    {
        int oldRemaining = RemainingRescues;

        rescuesUsed = Mathf.Clamp(newValue, 0, rescueUses);

        int newRemaining = RemainingRescues;

        if (newRemaining != oldRemaining)
        OnRemainingRescuesChanged?.Invoke(newRemaining);
    }

    public void ResetRescues()
    {
        SetRescuesUsed(0);
    }
}