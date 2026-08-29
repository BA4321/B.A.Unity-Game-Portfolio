using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIKenBurnsEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform targetRect;

    [Header("Timing")]
    [SerializeField] private float segmentDuration = 8f;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Zoom Per Point")]
    [Tooltip("Scale values matched by index with offsetPoints. If fewer scales are provided, missing ones use 1.")]
    [SerializeField] private float[] scalePoints = new float[] { 1.08f, 1.12f, 1.1f, 1.15f, 1.09f };

    [Header("Pan Points")]
    [Tooltip("The image will move from point 0 -> 1 -> 2 -> ...")]
    [SerializeField] private Vector2[] offsetPoints = new Vector2[]
    {
        new Vector2(-40f, -20f),
        new Vector2( 30f, -10f),
        new Vector2( 45f,  25f),
        new Vector2(-20f,  30f),
        new Vector2(  0f,   0f)
    };

    [Header("Easing")]
    [SerializeField] private bool easeInOut = true;

    private int currentIndex = 0;
    private float timer = 0f;

    private void Reset()
    {
        targetRect = GetComponent<RectTransform>();
    }

    private void Awake()
    {
        if (targetRect == null)
            targetRect = GetComponent<RectTransform>();

        ApplyPointInstant(0);
    }

    private void Update()
    {
        if (targetRect == null)
            return;

        if (offsetPoints == null || offsetPoints.Length == 0)
            return;

        if (offsetPoints.Length == 1)
        {
            ApplyPointInstant(0);
            return;
        }

        if (segmentDuration <= 0f)
        {
            ApplyPointInstant(currentIndex);
            return;
        }

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        timer += dt;

        float t = Mathf.Clamp01(timer / segmentDuration);

        if (easeInOut)
            t = Mathf.SmoothStep(0f, 1f, t);

        int nextIndex = currentIndex + 1;

        if (nextIndex >= offsetPoints.Length)
        {
            nextIndex = loop ? 0 : offsetPoints.Length - 1;
        }

        Vector2 pos = Vector2.LerpUnclamped(offsetPoints[currentIndex], offsetPoints[nextIndex], t);
        float scale = Mathf.LerpUnclamped(GetScaleAt(currentIndex), GetScaleAt(nextIndex), t);

        targetRect.anchoredPosition = pos;
        targetRect.localScale = new Vector3(scale, scale, 1f);

        if (timer >= segmentDuration)
        {
            timer = 0f;

            if (currentIndex < offsetPoints.Length - 1)
            {
                currentIndex++;
            }
            else
            {
                if (loop)
                    currentIndex = 0;
                else
                    currentIndex = offsetPoints.Length - 1;
            }
        }
    }

    private float GetScaleAt(int index)
    {
        if (scalePoints == null || index < 0 || index >= scalePoints.Length)
            return 1f;

        return scalePoints[index];
    }

    private void ApplyPointInstant(int index)
    {
        index = Mathf.Clamp(index, 0, offsetPoints.Length - 1);

        targetRect.anchoredPosition = offsetPoints[index];
        float scale = GetScaleAt(index);
        targetRect.localScale = new Vector3(scale, scale, 1f);
    }

    public void Restart()
    {
        currentIndex = 0;
        timer = 0f;
        ApplyPointInstant(0);
    }
}