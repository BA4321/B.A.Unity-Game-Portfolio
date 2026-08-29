using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D_Mobile : MonoBehaviour
{

    /* ───────────────────────────── Movement ───────────────────────────── */
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField, Range(0.1f, 1f)] private float horizMaxAt = 0.8f;
    [SerializeField] private float airAccel = 10f;
    [SerializeField] private float maxFallSpeed = -20f;

    /* ───────────────────────────── Jumping ────────────────────────────── */
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.15f;
    [SerializeField] private float joystickJumpThreshold = 0.5f;

    [Tooltip("Set to 2 for Double Jump, 1 for Single Jump.")]
    [SerializeField] private int maxJumps = 2;

    /* ───────────────────────────── Input ──────────────────────────────── */
    [Header("Input")]
    [SerializeField] private ControlMode controlMode = ControlMode.Joystick;
    [SerializeField] private SimpleJoystick joystick;
    [Tooltip("This GameObject will be disabled when Control Mode is Keyboard.")]
    [SerializeField] private GameObject keyboardDisabledObject;

    /* ───────────────────────────── Animator ───────────────────────────── */
    [Header("Animator")]
    [SerializeField] private Animator animator;

    /* ───────────────────────────── Facing Tracking Only ───────────────── */
    [Header("Facing Tracking (no visual flip)")]
    [SerializeField] private float facingDeadzone = 0.05f;

    /* ───────────────────────────── particles ──────────────────────────── */
    [Header("Particle Effects")]
    [SerializeField] public ParticleSystem jumpeffect;
    [SerializeField] public ParticleSystem jumptraileffect;

    /* ───────────────────────────── Internals ──────────────────────────── */
    private Rigidbody2D rb;
    private int groundMask;

    private bool stickWasUpLastFrame;
    private float cachedHoriz;

    private int jumpCount;
    private bool wasGrounded;

    private bool facingLeft;

    public float LastMoveX { get; private set; }
    public bool FacingLeft => facingLeft;
    public int FacingSign => facingLeft ? -1 : 1;

    // Animator parameter hashes
    private static readonly int AnimGrounded = Animator.StringToHash("Grounded");
    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimYVel = Animator.StringToHash("YVel");
    


    private void Start()
    {
    ApplySavedControlMode();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundMask = LayerMask.GetMask("ground");

        if (!animator)
            animator = GetComponentInChildren<Animator>();

        ApplyControlModeObject();
    }

    private void Update()
    {
        float rawH = ReadHorizontalInput();
        float rawV = ReadVerticalInput();

        float absH = Mathf.Abs(rawH);
        float horiz = absH >= horizMaxAt ? Mathf.Sign(rawH) : rawH / Mathf.Max(0.0001f, horizMaxAt);
        cachedHoriz = Mathf.Clamp(horiz, -1f, 1f);
        LastMoveX = cachedHoriz;

        UpdateFacingTracking(rawH);

        bool groundedNow = IsGrounded;
        if (groundedNow && !wasGrounded)
            jumpCount = 0;

        bool jumpPressed = ReadJumpPressed(rawH, rawV);

        if (jumpPressed && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
            JumpEffectPlay();
        }

        wasGrounded = groundedNow;
        UpdateAnimator(groundedNow);
    }

    private void FixedUpdate()
    {
        float targetX = cachedHoriz * moveSpeed;

        if (!IsGrounded)
        {
            float newX = Mathf.Lerp(rb.linearVelocity.x, targetX, airAccel * Time.fixedDeltaTime);
            float newY = Mathf.Max(rb.linearVelocity.y, maxFallSpeed);
            rb.linearVelocity = new Vector2(newX, newY);
        }
        else
        {
            rb.linearVelocity = new Vector2(targetX, rb.linearVelocity.y);
        }
    }

    /* ───────────────────────────── Input Helpers ──────────────────────── */
    private float ReadHorizontalInput()
    {
        switch (controlMode)
        {
            case ControlMode.Keyboard:
            {
                float x = 0f;

                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                    x -= 1f;

                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                    x += 1f;

                return Mathf.Clamp(x, -1f, 1f);
            }

            case ControlMode.Joystick:
            default:
                return joystick ? joystick.Horizontal : 0f;
        }
    }

    private float ReadVerticalInput()
    {
        switch (controlMode)
        {
            case ControlMode.Keyboard:
            {
                float y = 0f;

                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                    y -= 1f;

                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                    y += 1f;

                return Mathf.Clamp(y, -1f, 1f);
            }

            case ControlMode.Joystick:
            default:
                return joystick ? joystick.Vertical : 0f;
        }
    }

    private bool ReadJumpPressed(float rawH, float rawV)
    {
        switch (controlMode)
        {
            case ControlMode.Keyboard:
                return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);

            case ControlMode.Joystick:
            default:
            {
                bool stickUp = rawV > joystickJumpThreshold;
                bool stickMostlyVertical = Mathf.Abs(rawH) < 0.7f;

                bool jumpPressed = stickUp && stickMostlyVertical && !stickWasUpLastFrame;
                stickWasUpLastFrame = stickUp && stickMostlyVertical;

                return jumpPressed;
            }
        }
    }

    public void SetControlMode(ControlMode newMode)
    {
        controlMode = newMode;
        stickWasUpLastFrame = false;
        cachedHoriz = 0f;
        ApplyControlModeObject();
    }

    public ControlMode GetControlMode()
    {
        return controlMode;
    }

    /* ───────────────────────────── Control Mode Object ────────────────── */
    private void ApplyControlModeObject()
    {
        if (!keyboardDisabledObject) return;
        keyboardDisabledObject.SetActive(controlMode != ControlMode.Keyboard);
    }

    /* ───────────────────────────── Animator Helper ────────────────────── */
    private void UpdateAnimator(bool groundedNow)
    {
        if (!animator) return;

        float speed = Mathf.Abs(rb.linearVelocity.x);
        float yVel = rb.linearVelocity.y;

        animator.SetBool(AnimGrounded, groundedNow);
        animator.SetFloat(AnimSpeed, speed);
        animator.SetFloat(AnimYVel, yVel);
    }

    /* ───────────────────────────── Helpers ────────────────────────────── */
    public bool IsGrounded =>
        groundCheck && Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);

    private void UpdateFacingTracking(float inputX)
    {
        if (Mathf.Abs(inputX) > facingDeadzone)
            facingLeft = inputX < 0f;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
#endif

    private void JumpEffectPlay()
    {
        if (jumpeffect) jumpeffect.Stop();
        if (jumptraileffect) jumptraileffect.Stop();
        if (jumptraileffect) jumptraileffect.Play();
        if (jumpeffect) jumpeffect.Play();
    }


    private void ApplySavedControlMode()
{
    if (InputModeManager.Instance != null && InputModeManager.Instance.HasChoice)
    {
        SetControlMode(InputModeManager.Instance.CurrentMode);
    }
}
}