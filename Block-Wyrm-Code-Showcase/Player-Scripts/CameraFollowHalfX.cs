using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to the Camera.
/// • Follows the target’s X-movement with a given ratio (default 50%).
/// • Drifts the camera on Y using riseSpeed * AnimationCurve(targetY - cameraY).
///   Curve can be negative -> camera goes down.
/// • Tracks how much the camera has moved on Y since start and can show it on a UI Text and/or UI Slider.
/// • Lets other scripts pause ONLY the vertical movement for a set duration (X-follow continues).
/// </summary>
public class CameraFollowHalfX : MonoBehaviour
{
    [Header("Horizontal Follow")]
    [SerializeField] private Transform target;      // Object to follow on X
    [Range(0f, 1f)] [SerializeField] private float ratio = 0.5f;

    [Header("Vertical Drift")]
    [Tooltip("Base vertical speed in units/sec. Final speed = riseSpeed * curveMultiplier.")]
    [SerializeField] private float riseSpeed = 0.5f; // units per second

    [Header("Rise Speed Multiplier Curve (SIGNED Y delta)")]
    [Tooltip("X = (targetY - cameraY). Positive means target is above the camera. Y = multiplier (can be negative).")]
    [SerializeField] private AnimationCurve yDeltaToMultiplier = new AnimationCurve(
        // Example defaults (edit in Inspector):
        // target is above -> speed up upward
        new Keyframe(10f, 1.5f),
        new Keyframe(5f, 2.0f),
        new Keyframe(0f, 1.0f),
        // target is below -> can slow/stop/go down
        new Keyframe(-5f, 0.0f),
        new Keyframe(-10f, -1.0f)
    );

    [Header("UI Text (optional)")]
    [Tooltip("Assign a uGUI Text (GameObject > UI > Text). Leave empty if you don't want on-screen display.")]
    [SerializeField] private Text riseText;
    [SerializeField] private string uiPrefix = "Rise: ";
    [SerializeField] private string uiSuffix = " u";
    [SerializeField] private int decimals = 1;

    [Header("UI Slider (optional)")]
    [Tooltip("Assign a uGUI Slider to visualize totalRiseY. Make it non-interactable.")]
    [SerializeField] private Slider riseSlider;
    [Tooltip("Minimum slider value (use 0 if you only expect positive rise).")]
    [SerializeField] private float sliderMinRise = 0f;
    [Tooltip("Maximum slider value shown initially.")]
    [SerializeField] private float sliderMaxRise = 50f;
    [Tooltip("If enabled, slider.maxValue auto-expands when totalRiseY exceeds it.")]
    [SerializeField] private bool autoExpandSliderMax = true;
    [Tooltip("When auto-expanding, add this padding on top of the current value.")]
    [SerializeField] private float autoExpandPadding = 5f;

    [Header("Runtime (read-only)")]
    [SerializeField, Tooltip("Net vertical distance added by this script since start (can be negative).")]
    private float totalRiseY = 0f;

    // Pause timer for ONLY the vertical rise (seconds left). Public getter for debug/other systems.
    public float RisePauseTimer => risePauseTimer;

    private float risePauseTimer = 0f;

    // Internals
    private float lastTargetX;
    private float fixedZ;

    // ----------------------------- Unity
    void Awake()
    {
        if (target != null) lastTargetX = target.position.x;
        fixedZ = transform.position.z;

        InitSliderIfAny();
        UpdateRiseUI();
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;

        // --- Horizontal follow with reduced effect ---
        if (target != null)
        {
            float deltaX = target.position.x - lastTargetX;
            pos.x += deltaX * ratio;
            lastTargetX = target.position.x;
        }

        // --- Vertical drift (pausable) ---
        if (risePauseTimer > 0f)
        {
            risePauseTimer -= Time.deltaTime;
            if (risePauseTimer < 0f) risePauseTimer = 0f;
        }
        else
        {
            float multiplier = 1f;

            if (target != null)
            {
                // SIGNED delta: negative means target is below the camera
                float yDelta = target.position.y - pos.y;
                multiplier = yDeltaToMultiplier.Evaluate(yDelta);
            }

            float currentVerticalSpeed = riseSpeed * multiplier; // can be negative
            float dy = currentVerticalSpeed * Time.deltaTime;

            pos.y += dy;
            totalRiseY += dy; // tracks net movement (can go down too)
        }

        // Keep Z unchanged
        pos.z = fixedZ;
        transform.position = pos;

        UpdateRiseUI();
    }

    void OnValidate()
    {
        // Slider sanity
        if (riseSlider != null)
        {
            if (sliderMaxRise < sliderMinRise) sliderMaxRise = sliderMinRise + 1f;
            riseSlider.minValue = sliderMinRise;
            riseSlider.maxValue = sliderMaxRise;
            riseSlider.wholeNumbers = false;
            riseSlider.interactable = false;
        }
    }

    // ----------------------------- Public API

    public float TotalRiseY => totalRiseY;

    public void PauseRise(float seconds)
    {
        if (seconds <= 0f) return;
        risePauseTimer += seconds;
    }

    public void SetRisePause(float seconds)
    {
        risePauseTimer = Mathf.Max(0f, seconds);
    }

    public void ResetRiseCounter()
    {
        totalRiseY = 0f;
        if (riseSlider != null)
        {
            riseSlider.value = riseSlider.minValue;
        }
        UpdateRiseUI();
    }

    public void SetRiseDisplayRange(float min, float max, bool clampCurrent = true)
    {
        if (riseSlider == null) return;
        if (max <= min) max = min + 1f;

        sliderMinRise = min;
        sliderMaxRise = max;

        riseSlider.minValue = min;
        riseSlider.maxValue = max;

        if (clampCurrent)
        {
            riseSlider.value = Mathf.Clamp(totalRiseY, min, max);
        }
        else
        {
            riseSlider.value = totalRiseY;
        }
    }

    // ----------------------------- Helpers

    private void InitSliderIfAny()
    {
        if (riseSlider == null) return;

        if (sliderMaxRise < sliderMinRise) sliderMaxRise = sliderMinRise + 1f;
        riseSlider.minValue = sliderMinRise;
        riseSlider.maxValue = sliderMaxRise;
        riseSlider.wholeNumbers = false;
        riseSlider.interactable = false;
        riseSlider.value = Mathf.Clamp(totalRiseY, riseSlider.minValue, riseSlider.maxValue);
    }

    private void UpdateRiseUI()
    {
        UpdateRiseLabel();
        UpdateRiseSlider();
    }

    private void UpdateRiseLabel()
    {
        if (riseText == null) return;

        float rounded = (decimals <= 0)
            ? Mathf.Round(totalRiseY)
            : Mathf.Round(totalRiseY * Mathf.Pow(10, decimals)) / Mathf.Pow(10, decimals);

        riseText.text = uiPrefix + rounded.ToString($"F{Mathf.Max(0, decimals)}") + uiSuffix;
    }

    private void UpdateRiseSlider()
    {
        if (riseSlider == null) return;

        if (autoExpandSliderMax && totalRiseY > riseSlider.maxValue)
        {
            float newMax = totalRiseY + Mathf.Max(0f, autoExpandPadding);
            sliderMaxRise = newMax;
            riseSlider.maxValue = newMax;
        }
        if (autoExpandSliderMax && totalRiseY < riseSlider.minValue)
        {
            float newMin = totalRiseY - Mathf.Max(0f, autoExpandPadding);
            sliderMinRise = newMin;
            riseSlider.minValue = newMin;
        }

        riseSlider.value = Mathf.Clamp(totalRiseY, riseSlider.minValue, riseSlider.maxValue);
    }
}
