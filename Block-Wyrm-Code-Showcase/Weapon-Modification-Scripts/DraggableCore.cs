using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableCore : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Save")]
    [Tooltip("Stable unique ID for this core prefab. Do not change after release.")]
    [SerializeField] private string saveId;

    [Header("Data")]
    [SerializeField] private EnergyCoreSO core;
    [SerializeField] public int MoneyCost;
    [SerializeField] public int GemCost;
    

    [Header("Shop")]
    [Tooltip("If enabled, this core acts as a shop-display item: selectable but not draggable.")]
    [SerializeField] private bool shopOnly;

    [Header("UI")]
    [SerializeField] private Image iconImage;

    [Tooltip("Top-most canvas to drag under (assign your DragCanvas here). If empty, will try find GameObject named 'DragCanvas'.")]
    [SerializeField] private Canvas dragCanvas;

    [Header("Drag Visuals")]
    [SerializeField, Range(1f, 1.5f)] private float liftScale = 1.1f;
    [SerializeField, Range(0.1f, 1f)] private float dragAlpha = 0.7f;

    public EnergyCoreSO Core => core;
    public bool IsShopOnly => shopOnly;
    public string SaveId => string.IsNullOrWhiteSpace(saveId) ? gameObject.name : saveId;

    public CoreSlot OriginSlot { get; private set; }

    public static Action<EnergyCoreSO> OnCoreSelected;
    public static Action<DraggableCore> OnCoreDragStarted;
    public static Action<DraggableCore> OnCoreDragEnded;

    public static DraggableCore CurrentSelected { get; private set; }
    public static DraggableCore CurrentDragging { get; private set; }

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private int originalSiblingIndex;

private float selectedTime = -1f;           
private const float deselectGuardSeconds = .10f; /*
                                    Sometimes , especially when game is started first, When player selects a core and clicks purchase button selection dissapears before  
                                    purchase button can be pressed. This happens because IsPointerOverGameObject returns false unreliably on the first few frames during editor startup
                                    and to get in front of this, and to fix the bug I made it so Deselection happens with a 0.x seconds delay
                                    */
    private Canvas cachedRootCanvas;

    private Vector3 originalScale;
    private float originalAlpha;

    private bool cachedInitialValues;

    private bool dragActive;

    private void Awake()
    {
        EnsureReferences();
        RefreshIcon();
    }

    private void EnsureReferences()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>(true);

        if (dragCanvas == null || cachedRootCanvas == null)
            ResolveDragCanvas();

        if (!cachedInitialValues)
        {
            originalScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
            originalAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
            cachedInitialValues = true;
        }
    }

    private void Update()
    {
        if (CurrentSelected != this) return;

        // Guard Rail that stops deselection for deselectGuardSeconds seconds
        if (Time.unscaledTime < selectedTime + deselectGuardSeconds) return;

        bool pointerDown = false;
        int pointerId = -1;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        if (Input.GetMouseButtonDown(0))
        {
            pointerDown = true;
            pointerId = -1;
        }
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            pointerDown = true;
            pointerId = Input.GetTouch(0).fingerId;
        }
#endif

        if (!pointerDown) return;
        if (EventSystem.current == null) return;

        if (!EventSystem.current.IsPointerOverGameObject(pointerId))
            Deselect();
    }

    public static void Deselect()
    {
       // if (CurrentSelected != null)
        //    Debug.Log($"Deselect called from:\n{new System.Diagnostics.StackTrace()}", CurrentSelected);

        CurrentSelected = null;
        OnCoreSelected?.Invoke(null);
    }

    private void OnValidate()
    {
        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>(true);

        RefreshIcon();
    }

    public void SetCore(EnergyCoreSO newCore)
    {
        core = newCore;
        RefreshIcon();
    }

    private void RefreshIcon()
    {
        if (iconImage == null) return;

        if (core != null && core.Icon != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = core.Icon;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    private void ResolveDragCanvas()
    {
        if (dragCanvas == null)
        {
            var go = GameObject.Find("DragCanvas");
            if (go != null)
                dragCanvas = go.GetComponent<Canvas>();
        }

        Canvas parentCanvas = GetComponentInParent<Canvas>(true);
        cachedRootCanvas = parentCanvas != null ? parentCanvas.rootCanvas : null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        dragActive = false;
        if (CurrentSelected == this)
            Deselect();
        else
            Select(this);
    }

    public static void Select(DraggableCore coreInstance)
    {
        CurrentSelected = coreInstance;
        if (coreInstance != null)
            coreInstance.selectedTime = Time.unscaledTime;
        OnCoreSelected?.Invoke(coreInstance != null ? coreInstance.Core : null);
    }

    public void OnBeginDrag(PointerEventData eventData)
{
    if (shopOnly)
    {
        dragActive = false;

        if (eventData != null)
            eventData.pointerDrag = null;

        if (CurrentDragging == this)
            CurrentDragging = null;

        return;
    }

    dragActive = true;

    EnsureReferences();

    originalParent = transform.parent;
    originalSiblingIndex = transform.GetSiblingIndex();

    OriginSlot = originalParent != null ? originalParent.GetComponent<CoreSlot>() : null;
    OriginSlot?.NotifyDragStarted(this);

    originalScale = rectTransform.localScale;
    originalAlpha = canvasGroup.alpha;

    rectTransform.localScale = originalScale * liftScale;
    canvasGroup.alpha = dragAlpha;

    CurrentDragging = this;

    if (dragCanvas != null)
    {
        transform.SetParent(dragCanvas.transform, worldPositionStays: false);
        transform.SetAsLastSibling();
    }

    canvasGroup.blocksRaycasts = false;

    OnCoreDragStarted?.Invoke(this);

    UpdateDragPosition(eventData);
}

    public void OnDrag(PointerEventData eventData)
{
    if (!dragActive || CurrentDragging != this)
        return;

    EnsureReferences();
    UpdateDragPosition(eventData);
}

    public void OnEndDrag(PointerEventData eventData)
{
    if (!dragActive || CurrentDragging != this)
        return;

    dragActive = false;

    EnsureReferences();

    canvasGroup.blocksRaycasts = true;

    rectTransform.localScale = originalScale;
    canvasGroup.alpha = originalAlpha;

    bool stillInDragCanvas = (dragCanvas != null && transform.parent == dragCanvas.transform);
    if (stillInDragCanvas)
        ReturnToOriginalParent();

    OriginSlot?.NotifyDragEnded(this);
    OriginSlot = null;

    if (CurrentDragging == this)
        CurrentDragging = null;

    OnCoreDragEnded?.Invoke(this);
}

    private void UpdateDragPosition(PointerEventData eventData)
    {
        EnsureReferences();

        Canvas canvasToUse = dragCanvas != null ? dragCanvas : cachedRootCanvas;
        if (canvasToUse == null)
        {
            rectTransform.position = eventData.position;
            return;
        }

        RectTransform canvasRect = canvasToUse.transform as RectTransform;
        if (canvasRect == null)
        {
            rectTransform.position = eventData.position;
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                eventData.position,
                canvasToUse.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvasToUse.worldCamera,
                out Vector2 localPoint))
        {
            rectTransform.localPosition = localPoint;
        }
    }

    public void SnapToSlot(Transform slotTransform)
    {
        EnsureReferences();

        if (slotTransform == null)
        {
            ReturnToOriginalParent();
            return;
        }

        if (rectTransform == null)
        {
            Debug.LogError($"DraggableCore '{name}' has no RectTransform.", this);
            return;
        }

        transform.SetParent(slotTransform, worldPositionStays: false);
        transform.SetAsLastSibling();

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
    }

    public void ReturnToOriginalParent()
    {
        EnsureReferences();

        if (originalParent == null)
            return;

        if (rectTransform == null)
        {
            Debug.LogError($"DraggableCore '{name}' has no RectTransform.", this);
            return;
        }

        transform.SetParent(originalParent, worldPositionStays: false);
        transform.SetSiblingIndex(originalSiblingIndex);

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
    }
}