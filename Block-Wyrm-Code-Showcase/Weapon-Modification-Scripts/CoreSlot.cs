using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CoreSlot : MonoBehaviour,
    IDropHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Slot Rules")]
    public WeaponType WeaponType = WeaponType.None; // None = Bag
    [Range(0, 99)] public int SlotIndex = 0;

    [Header("Bag Slot Locking")]
    [Tooltip("Enable this only for bag slots that can be locked/unlocked.")]
    [SerializeField] private bool useBagSlotLocking = false;

    [Tooltip("Stable save index for this bag slot. Example: 0, 1, 2, 3.")]
    [SerializeField, Min(0)] private int bagSlotIndex = 0;

    [Tooltip("Used only when there is no saved unlock data yet.")]
    [SerializeField] private bool defaultUnlocked = true;

    [Tooltip("Gem cost required to unlock this slot.")]
    [SerializeField, Min(0)] private int unlockGemCost = 10;

    [Tooltip("Visual object shown while this slot is locked.")]
    [SerializeField] private GameObject lockOverlay;

    [Tooltip("Text showing the gem unlock cost while locked.")]
    [SerializeField] private TMP_Text unlockCostText;

    [Header("Core Visuals")]
    [SerializeField] private Vector3 slottedCoreScale = Vector3.one;

    [Header("Selection UI")]
    [SerializeField] private GameObject SelectionHighlight;

    [Header("Hover Feedback")]
    [Tooltip("Background graphic to tint on hover (Image/Text/etc). If null, will try GetComponent<Graphic>().")]
    [SerializeField] private Graphic slotGraphic;
    [SerializeField] private Color hoverValidColor = Color.green;
    [SerializeField] private Color hoverInvalidColor = Color.red;

    private Color baseColor;
    private bool hasBaseColor;

    /// <summary>
    /// Fired whenever an item is dropped into or removed from a weapon slot (WeaponType != None).
    /// Includes moving between weapon slots, for example Slot 0 -> Slot 1.
    /// </summary>
    public static Action OnInventoryChanged;

    /// <summary>
    /// Fired when the player clicks a locked bag slot.
    /// The unlock confirmation panel should listen to this later.
    /// </summary>
    public static Action<CoreSlot> OnLockedBagSlotClicked;

    /// <summary>
    /// Fired when this slot changes locked/unlocked state.
    /// Useful for UI refreshes later.
    /// </summary>
    public static Action<CoreSlot> OnBagSlotLockStateChanged;

    // Tracks the core that was dragged OUT of this slot.
    // Needed because it gets re-parented to DragCanvas during drag.
    private DraggableCore draggingOutCore;

    public bool IsBagSlot => WeaponType == WeaponType.None;

    public bool UsesBagSlotLocking => IsBagSlot && useBagSlotLocking;

    public bool IsLockableBagSlot => UsesBagSlotLocking;

    public int BagSlotIndex => bagSlotIndex;

    // Kept compatible with the previous save initializer step.
    public int BagSlotUnlockIndex => bagSlotIndex;

    public bool DefaultUnlocked => defaultUnlocked;

    // Kept compatible with the previous save initializer step.
    public bool InspectorDefaultUnlocked => defaultUnlocked;

    public int UnlockGemCost => unlockGemCost;

    public bool IsUnlocked { get; private set; } = true;

    public bool IsLocked => !IsUnlocked;

    public bool HasOccupant => GetOccupant() != null;

    private void Awake()
    {
        if (slotGraphic == null)
            slotGraphic = GetComponent<Graphic>();

        if (slotGraphic != null)
        {
            baseColor = slotGraphic.color;
            hasBaseColor = true;
        }

        if (UsesBagSlotLocking)
            IsUnlocked = defaultUnlocked;
        else
            IsUnlocked = true;

        RefreshLockVisuals();
    }

    private void OnEnable()
    {
        DraggableCore.OnCoreSelected += HandleCoreSelected;
        DraggableCore.OnCoreDragStarted += HandleCoreDragStarted;
        DraggableCore.OnCoreDragEnded += HandleCoreDragEnded;
        OnInventoryChanged += HandleInventoryChanged;

        UpdateHighlight();
        ResetHoverColor();
        ApplyScaleToOccupant();
        RefreshLockVisuals();
    }

    private void OnDisable()
    {
        DraggableCore.OnCoreSelected -= HandleCoreSelected;
        DraggableCore.OnCoreDragStarted -= HandleCoreDragStarted;
        DraggableCore.OnCoreDragEnded -= HandleCoreDragEnded;
        OnInventoryChanged -= HandleInventoryChanged;
    }

    private void OnValidate()
    {
        // Only bag slots should use bag slot locking.
        if (WeaponType != WeaponType.None)
            useBagSlotLocking = false;

        if (unlockCostText != null)
            unlockCostText.text = unlockGemCost.ToString();

        RefreshLockVisuals();
    }

    // ----------------- LOCK STATE -----------------

    public void ApplySavedBagSlotUnlockedState(bool unlocked)
    {
        if (!UsesBagSlotLocking)
        {
            IsUnlocked = true;
            RefreshLockVisuals();
            return;
        }

        IsUnlocked = unlocked;
        RefreshLockVisuals();
        OnBagSlotLockStateChanged?.Invoke(this);
    }

    public void SaveBagSlotUnlockedState(bool unlocked)
    {
        if (!UsesBagSlotLocking)
            return;

        bool changed = IsUnlocked != unlocked;

        ApplySavedBagSlotUnlockedState(unlocked);

        if (changed && GameManager.Instance != null)
            GameManager.Instance.SetBagSlotUnlocked(BagSlotUnlockIndex, unlocked);
    }

    public void UnlockAndSave()
    {
        SaveBagSlotUnlockedState(true);
    }

    public void LockAndSave()
    {
        SaveBagSlotUnlockedState(false);
    }

    private void RefreshLockVisuals()
    {
        bool showLockedVisual = UsesBagSlotLocking && IsLocked;

        if (lockOverlay != null)
            lockOverlay.SetActive(showLockedVisual);

        if (unlockCostText != null)
        {
            unlockCostText.text = unlockGemCost.ToString();
            unlockCostText.gameObject.SetActive(showLockedVisual);
        }

        if (SelectionHighlight != null && showLockedVisual)
            SelectionHighlight.SetActive(false);
    }

    private void RequestUnlockPanel()
    {
        if (!UsesBagSlotLocking)
            return;

        if (IsUnlocked)
            return;

        OnLockedBagSlotClicked?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!UsesBagSlotLocking || IsUnlocked)
            return;

        RequestUnlockPanel();

        if (eventData != null)
            eventData.Use();
    }

    // ----------------- SAVE / LOADOUT -----------------

    private static void PersistWeaponSlots(CoreSlot a, CoreSlot b)
    {
        if (UIWeaponLoadoutRuntime.Instance == null) return;

        // SyncFromSlot ignores Bag slots automatically.
        if (a != null) UIWeaponLoadoutRuntime.Instance.SyncFromSlot(a);
        if (b != null) UIWeaponLoadoutRuntime.Instance.SyncFromSlot(b);
    }

    public void NotifyDragStarted(DraggableCore core)
    {
        draggingOutCore = core;
        UpdateHighlight();
    }

    public void NotifyDragEnded(DraggableCore core)
    {
        if (draggingOutCore == core)
            draggingOutCore = null;

        UpdateHighlight();
        ApplyScaleToOccupant();
    }

    private void HandleCoreSelected(EnergyCoreSO _) => UpdateHighlight();

    private void HandleInventoryChanged()
    {
        UpdateHighlight();
        ApplyScaleToOccupant();
    }

    private void HandleCoreDragStarted(DraggableCore _) => UpdateHighlight();

    private void HandleCoreDragEnded(DraggableCore _)
    {
        UpdateHighlight();
        ApplyScaleToOccupant();
    }

    private DraggableCore GetOccupant()
    {
        return GetComponentInChildren<DraggableCore>(includeInactive: false);
    }

    private void UpdateHighlight()
    {
        if (SelectionHighlight == null) return;

        if (UsesBagSlotLocking && IsLocked)
        {
            SelectionHighlight.SetActive(false);
            return;
        }

        bool isDraggingOut = draggingOutCore != null;

        var occupant = isDraggingOut ? null : GetOccupant();
        bool selectedHere = occupant != null && occupant == DraggableCore.CurrentSelected;

        SelectionHighlight.SetActive(selectedHere);

        if (selectedHere)
            SelectionHighlight.transform.SetAsLastSibling();
    }

    private void ApplyScaleToOccupant()
    {
        var occupant = GetOccupant();
        if (occupant == null) return;

        occupant.transform.localScale = slottedCoreScale;
    }

    private void ApplyScaleToCore(DraggableCore core)
    {
        if (core == null) return;

        core.transform.localScale = slottedCoreScale;
    }

    // ----------------- HOVER FEEDBACK -----------------

    public void OnPointerEnter(PointerEventData eventData)
    {
        var dragging = DraggableCore.CurrentDragging;
        if (dragging == null) return;

        bool valid = AcceptsCore(this, dragging.Core);
        SetHoverColor(valid ? hoverValidColor : hoverInvalidColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetHoverColor();
    }

    private void SetHoverColor(Color c)
    {
        if (slotGraphic == null) return;

        slotGraphic.color = c;
    }

    private void ResetHoverColor()
    {
        if (slotGraphic == null || !hasBaseColor) return;

        slotGraphic.color = baseColor;
    }

    // ----------------- DROP / SWAP -----------------

    public void OnDrop(PointerEventData eventData)
    {
        ResetHoverColor();

        if (eventData == null) return;

        var draggedObj = eventData.pointerDrag;
        if (draggedObj == null) return;

        var coreA = draggedObj.GetComponent<DraggableCore>();
        if (coreA == null) return;

        CoreSlot start = coreA.OriginSlot;
        CoreSlot end = this;

        var coreB = end.GetOccupant();

        if (!AcceptsCore(end, coreA.Core))
        {
            coreA.ReturnToOriginalParent();

            if (start != null)
                start.ApplyScaleToOccupant();

            return;
        }

        if (coreB != null)
        {
            if (start == null)
            {
                coreA.ReturnToOriginalParent();
                return;
            }

            if (!AcceptsCore(start, coreB.Core))
            {
                coreA.ReturnToOriginalParent();
                start.ApplyScaleToOccupant();
                end.ApplyScaleToOccupant();
                return;
            }

            SwapCores(start, end);
            PersistWeaponSlots(start, end);
            FireInventoryChangedIfNeeded(start, end);
            return;
        }

        coreA.SnapToSlot(end.transform);
        end.ApplyScaleToCore(coreA);

        PersistWeaponSlots(start, end);

        if (start != null && start != end)
            FireInventoryChangedIfNeeded(start, end);
        else if (start == null && end.WeaponType != WeaponType.None)
            OnInventoryChanged?.Invoke();
    }

    public bool CanAcceptCore(EnergyCoreSO core)
    {
        return AcceptsCore(this, core);
    }

    private static bool AcceptsCore(CoreSlot slot, EnergyCoreSO core)
    {
        if (slot == null)
            return false;

        // Locked bag slots reject drops.
        if (slot.UsesBagSlotLocking && slot.IsLocked)
            return false;

        // Bag slots accept any core, as long as they are unlocked.
        if (slot.WeaponType == WeaponType.None)
            return true;

        // Weapon slots accept only compatible cores.
        return core != null && core.compatibleWeaponType == slot.WeaponType;
    }

    private static void FireInventoryChangedIfNeeded(CoreSlot start, CoreSlot end)
    {
        if (start == null || end == null) return;
        if (start == end) return;

        bool startWeapon = start.WeaponType != WeaponType.None;
        bool endWeapon = end.WeaponType != WeaponType.None;

        if (startWeapon || endWeapon)
            OnInventoryChanged?.Invoke();
    }

    public static void SwapCores(CoreSlot start, CoreSlot end)
    {
        if (start == null || end == null || start == end) return;

        DraggableCore coreA = start.draggingOutCore;
        DraggableCore coreB = end.GetComponentInChildren<DraggableCore>(includeInactive: false);

        if (coreA == null || coreB == null) return;

        coreB.SnapToSlot(start.transform);
        start.ApplyScaleToCore(coreB);

        coreA.SnapToSlot(end.transform);
        end.ApplyScaleToCore(coreA);
    }
}