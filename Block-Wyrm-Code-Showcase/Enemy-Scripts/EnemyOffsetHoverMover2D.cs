using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyOffsetHoverMover2D : MonoBehaviour
{
    public enum MoveState { Pursuing, Maintaining, Retreating }

    [Header("Target (Player)")]
    [SerializeField] private Transform target; // optional override for testing

    [SerializeField] private EnemyRangedAttack2D rangedAttack;

    [Header("Graphics")]
    [SerializeField] private Transform graphicsChild;
    [SerializeField] private Animator _animator;

    [Header("Attack")]
    [SerializeField] private float maintainAttackCooldown = 5f;

    [Header("Preferred Offset (relative to player)")]
    [SerializeField] private float baseYAbovePlayer = 2.0f;
    [SerializeField] private float yJitter = 0.25f;
    [SerializeField] private Vector2 stopDistanceRange = new Vector2(2.5f, 4.0f);

    [Header("Maintain Offset Randomization")]
    [SerializeField] private Vector2 maintainOffsetJitter = new Vector2(0.4f, 0.2f);
    [SerializeField] private Vector2 maintainOffsetRetargetTimeRange = new Vector2(0.75f, 1.5f);
    [SerializeField] private float maintainOffsetChangeSpeed = 2.5f;

    [Header("Retreat Hysteresis")]
    [SerializeField] private float retreatEnterDistance = 1.25f;
    [SerializeField] private float retreatReleaseDistance = 2.25f;
    [SerializeField] private float retreatExtra = 1.0f;

    [Header("Maintaining Deadzones")]
    [SerializeField] private float maintainDeadzoneX = 0.15f;
    [SerializeField] private float maintainDeadzoneY = 0.15f;

    [Header("Speeds (units/sec)")]
    [SerializeField] private float pursueHorizontalSpeed = 6f;
    [SerializeField] private float pursueVerticalSpeed = 4f;

    [SerializeField] private float maintainHorizontalSpeed = 2.5f;
    [SerializeField] private float maintainVerticalSpeed = 2.5f;

    [SerializeField] private float retreatHorizontalSpeed = 7f;
    [SerializeField] private float retreatVerticalSpeed = 5f;

    [Header("Tuning")]
    [SerializeField] private float gain = 2.0f;
    [SerializeField] private float acceleration = 25f;

    [Header("Side Selection (Anti-Joust)")]
    [SerializeField] private float noCrossDistance = 2.0f;
    [SerializeField] private float sideDeadzone = 0.25f;
    [SerializeField] private float sideSwitchHysteresis = 0.5f;
    [SerializeField] private float sideSwitchCooldown = 0.25f;

    [Header("Debug")]
    [SerializeField] private MoveState state;
    [SerializeField] private bool drawGizmos = true;

    private Rigidbody2D rb;

    private bool isAttackPaused = false; //manually inserted code

    private float stopDistance;
    private float personalYAbove;

    private Vector2 currentMaintainOffset;
    private Vector2 targetMaintainOffset;
    private float nextMaintainOffsetPickTime;

    private int preferredSide = 1;
    private float nextAllowedSideSwitchTime = 0f;

    private float nextAttackTime = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ResetMaintainOffsetImmediate();

        if (retreatReleaseDistance <= retreatEnterDistance)
            retreatReleaseDistance = retreatEnterDistance + 1.0f;
    }

    private void OnEnable()
    {
        TryResolveTargetFromSingleton();
        ResetMaintainOffsetImmediate();
        PickPersonalOffsets();

        nextAttackTime = 0f; // allows immediate shot first time it reaches Maintaining
        UpdateGraphicsFacing();
    }

    private void Start()
    {
        TryResolveTargetFromSingleton();

        if (target != null)
        {
            float dx = rb.position.x - (float)target.position.x;
            preferredSide = (Mathf.Abs(dx) < 0.001f) ? (Random.value < 0.5f ? -1 : 1) : (dx >= 0f ? 1 : -1);
        }

        state = MoveState.Pursuing;
        UpdateGraphicsFacing();
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            TryResolveTargetFromSingleton();
            if (target == null) return;
        }

        Vector2 pos = rb.position;
        Vector2 p = target.position;

        float distToPlayer = Vector2.Distance(pos, p);

        UpdatePreferredSide_NoJoust(pos, p, distToPlayer);
        UpdateGraphicsFacing();
        UpdateMaintainOffset();

        Vector2 desiredPos = new Vector2(
            p.x + preferredSide * stopDistance + currentMaintainOffset.x,
            p.y + personalYAbove + currentMaintainOffset.y
        );

        if (state == MoveState.Retreating)
        {
            if (distToPlayer > retreatReleaseDistance)
            {
                float ex = desiredPos.x - pos.x;
                float ey = desiredPos.y - pos.y;
                bool inDeadzone = Mathf.Abs(ex) <= maintainDeadzoneX && Mathf.Abs(ey) <= maintainDeadzoneY;
                state = inDeadzone ? MoveState.Maintaining : MoveState.Pursuing;
            }
        }
        else
        {
            if (distToPlayer < retreatEnterDistance)
            {
                state = MoveState.Retreating;
            }
            else
            {
                float ex = desiredPos.x - pos.x;
                float ey = desiredPos.y - pos.y;
                bool inDeadzone = Mathf.Abs(ex) <= maintainDeadzoneX && Mathf.Abs(ey) <= maintainDeadzoneY;
                state = inDeadzone ? MoveState.Maintaining : MoveState.Pursuing;
            }
        }

        Vector2 desiredVel;

        switch (state)
        {
            case MoveState.Pursuing:
            {
                float ex = desiredPos.x - pos.x;
                float ey = desiredPos.y - pos.y;

                desiredVel = new Vector2(
                    Mathf.Clamp(ex * gain, -pursueHorizontalSpeed, pursueHorizontalSpeed),
                    Mathf.Clamp(ey * gain, -pursueVerticalSpeed, pursueVerticalSpeed)
                );
                break;
            }

            case MoveState.Maintaining:
            {
                float ex = desiredPos.x - pos.x;
                float ey = desiredPos.y - pos.y;

                float vx = Mathf.Clamp(ex * gain, -maintainHorizontalSpeed, maintainHorizontalSpeed);
                float vy = Mathf.Clamp(ey * gain, -maintainVerticalSpeed, maintainVerticalSpeed);

                if (Mathf.Abs(ex) <= maintainDeadzoneX) vx = 0f;
                if (Mathf.Abs(ey) <= maintainDeadzoneY) vy = 0f;

                StartCoroutine(TryFireAtPlayer());

                desiredVel = new Vector2(vx, vy);
                break;
            }

            case MoveState.Retreating:
            default:
            {
                int awaySide = (pos.x - p.x) >= 0f ? 1 : -1;

                float retreatTargetDist = stopDistance + retreatExtra;
                Vector2 retreatPos = new Vector2(
                    p.x + awaySide * retreatTargetDist,
                    p.y + personalYAbove
                );

                float ex = retreatPos.x - pos.x;
                float ey = retreatPos.y - pos.y;

                desiredVel = new Vector2(
                    Mathf.Clamp(ex * gain, -retreatHorizontalSpeed, retreatHorizontalSpeed),
                    Mathf.Clamp(ey * gain, -retreatVerticalSpeed, retreatVerticalSpeed)
                );
                break;
            }
        }

        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, desiredVel, acceleration * Time.fixedDeltaTime);
    }

    private IEnumerator TryFireAtPlayer()
    {
        if (rangedAttack == null) yield break;
        if (Time.time < nextAttackTime) yield break;
        if (isAttackPaused) yield break;  // gate
        
        isAttackPaused = true;  // close the gate
        
        _animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.25f);  // waits HERE before firing

        rangedAttack.FireAtPlayer();
        yield return new WaitForSeconds(0.25f);  // waits HERE after firing

        nextAttackTime = Time.time + maintainAttackCooldown;
        isAttackPaused = false;  // reopen the gate
    }

    private void TryResolveTargetFromSingleton()
    {
        if (target != null) return;

        if (PlayerHealthManager.Instance != null)
            target = PlayerHealthManager.Instance.transform;
    }

    private void PickPersonalOffsets()
    {
        float min = Mathf.Min(stopDistanceRange.x, stopDistanceRange.y);
        float max = Mathf.Max(stopDistanceRange.x, stopDistanceRange.y);

        stopDistance = Random.Range(min, max);
        personalYAbove = baseYAbovePlayer + Random.Range(-yJitter, yJitter);
        if (personalYAbove < 0.05f) personalYAbove = 0.05f;
    }

    private void UpdateMaintainOffset()
    {
        if (state == MoveState.Maintaining)
        {
            if (Time.time >= nextMaintainOffsetPickTime)
            {
                targetMaintainOffset = new Vector2(
                    Random.Range(-maintainOffsetJitter.x, maintainOffsetJitter.x),
                    Random.Range(-maintainOffsetJitter.y, maintainOffsetJitter.y)
                );

                float minTime = Mathf.Min(maintainOffsetRetargetTimeRange.x, maintainOffsetRetargetTimeRange.y);
                float maxTime = Mathf.Max(maintainOffsetRetargetTimeRange.x, maintainOffsetRetargetTimeRange.y);
                nextMaintainOffsetPickTime = Time.time + Random.Range(minTime, maxTime);
            }
        }
        else
        {
            targetMaintainOffset = Vector2.zero;
        }

        currentMaintainOffset = Vector2.MoveTowards(
            currentMaintainOffset,
            targetMaintainOffset,
            maintainOffsetChangeSpeed * Time.fixedDeltaTime
        );
    }

    private void ResetMaintainOffsetImmediate()
    {
        currentMaintainOffset = Vector2.zero;
        targetMaintainOffset = Vector2.zero;
        nextMaintainOffsetPickTime = 0f;
    }

    private void UpdatePreferredSide_NoJoust(Vector2 enemyPos, Vector2 playerPos, float distToPlayer)
    {
        int enemySideNow = SideFromRelativeX(enemyPos.x - playerPos.x, sideDeadzone);
        if (distToPlayer <= noCrossDistance && enemySideNow != 0)
        {
            preferredSide = enemySideNow;
            return;
        }

        if (Time.time < nextAllowedSideSwitchTime) return;

        float leftSlotX = playerPos.x - stopDistance;
        float rightSlotX = playerPos.x + stopDistance;

        float costLeft = Mathf.Abs(leftSlotX - enemyPos.x);
        float costRight = Mathf.Abs(rightSlotX - enemyPos.x);

        int bestSide = (costLeft <= costRight) ? -1 : 1;

        if (bestSide == preferredSide) return;

        float currentCost = (preferredSide == -1) ? costLeft : costRight;
        float bestCost = (bestSide == -1) ? costLeft : costRight;

        if ((currentCost - bestCost) >= sideSwitchHysteresis)
        {
            preferredSide = bestSide;
            nextAllowedSideSwitchTime = Time.time + sideSwitchCooldown;
        }
    }

    private void UpdateGraphicsFacing()
    {
        if (graphicsChild == null) return;

        // preferredSide = 1  -> enemy stays on player's right side, so face left
        // preferredSide = -1 -> enemy stays on player's left side, so face right
        float yRotation = (preferredSide == 1) ? 180f : 0f;

        Vector3 localEuler = graphicsChild.localEulerAngles;
        localEuler.y = yRotation;
        graphicsChild.localEulerAngles = localEuler;
    }

    private static int SideFromRelativeX(float relX, float deadzone)
    {
        if (relX > deadzone) return 1;
        if (relX < -deadzone) return -1;
        return 0;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, retreatEnterDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, retreatReleaseDistance);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, noCrossDistance);
    }
    private IEnumerator AttackMovementPause(float duration)
    {
        isAttackPaused = true;
        yield return new WaitForSeconds(duration);
        isAttackPaused = false;
    }


}