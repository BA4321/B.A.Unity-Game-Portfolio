using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum CoreGrantResult
{
    Success,
    InvalidCore,
    MissingPrefabRegistry,
    MissingCorePrefab,
    BusyDragging,
    NoSpace
}

public class UICoreGrantService : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UICorePrefabRegistry prefabRegistry;

    [Header("Slots")]
    [SerializeField] private List<CoreSlot> slots = new List<CoreSlot>();
    [SerializeField] private bool autoFindSlotsIfListEmpty = true;

    [Header("Placement Rules")]
    [SerializeField] private bool allowBagFallback = true;

    [Header("Events")]
    [Tooltip("Useful for calling SaveToDisk() after a successful grant.")]
    [SerializeField] private UnityEvent onGrantSucceeded;

    public IReadOnlyList<CoreSlot> Slots => slots;

    private void Awake()
    {
        CacheSlotsIfNeeded();
    }

    [ContextMenu("Refresh Slots From Children")]
    public void RefreshSlotsFromChildren()
    {
        CoreSlot[] found = GetComponentsInChildren<CoreSlot>(true);
        slots = new List<CoreSlot>(found);
    }

    public CoreGrantResult CanGrantCore(EnergyCoreSO core)
    {
        return CanGrantCore(core, out _);
    }

    public CoreGrantResult CanGrantCore(EnergyCoreSO core, out CoreSlot suggestedSlot)
    {
        CacheSlotsIfNeeded();
        suggestedSlot = null;

        if (core == null)
            return CoreGrantResult.InvalidCore;

        if (prefabRegistry == null)
            return CoreGrantResult.MissingPrefabRegistry;

        if (!prefabRegistry.TryGetPrefab(core, out _))
            return CoreGrantResult.MissingCorePrefab;

        // Prevent grant while user is dragging something, otherwise a dragged-out slot may look empty.
        if (DraggableCore.CurrentDragging != null)
            return CoreGrantResult.BusyDragging;

        if (!TryFindBestEmptySlot(core, out suggestedSlot))
            return CoreGrantResult.NoSpace;

        return CoreGrantResult.Success;
    }

    public CoreGrantResult TryGrantCore(EnergyCoreSO core)
    {
        return TryGrantCore(core, out _, out _);
    }

    public CoreGrantResult TryGrantCore(EnergyCoreSO core, out DraggableCore createdCore, out CoreSlot placedSlot)
    {
        createdCore = null;
        placedSlot = null;

        CoreGrantResult canGrant = CanGrantCore(core, out placedSlot);
        if (canGrant != CoreGrantResult.Success)
            return canGrant;

        if (!prefabRegistry.TryGetPrefab(core, out DraggableCore prefab) || prefab == null)
            return CoreGrantResult.MissingCorePrefab;

        createdCore = Instantiate(prefab, placedSlot.transform);
        createdCore.SnapToSlot(placedSlot.transform);

        // Keep weapon runtime in sync if the core was placed directly into a weapon slot.
        if (placedSlot.WeaponType != WeaponType.None)
        {
            if (UIWeaponLoadoutRuntime.Instance != null)
                UIWeaponLoadoutRuntime.Instance.SyncFromSlot(placedSlot);

            CoreSlot.OnInventoryChanged?.Invoke();
        }

        onGrantSucceeded?.Invoke();
        return CoreGrantResult.Success;
    }

    private void CacheSlotsIfNeeded()
    {
        if (slots != null && slots.Count > 0)
            return;

        if (!autoFindSlotsIfListEmpty)
            return;

        RefreshSlotsFromChildren();
    }

    private bool TryFindBestEmptySlot(EnergyCoreSO core, out CoreSlot slot)
    {
        slot = null;
        if (core == null)
            return false;

        // 1) First empty compatible weapon slot
        for (int i = 0; i < slots.Count; i++)
        {
            CoreSlot candidate = slots[i];
            if (candidate == null)
                continue;

            if (candidate.WeaponType == WeaponType.None)
                continue;

            if (candidate.WeaponType != core.compatibleWeaponType)
                continue;

            if (!IsSlotEmpty(candidate))
                continue;

            slot = candidate;
            return true;
        }

        // 2) If allowed, first empty bag slot
        if (allowBagFallback)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                CoreSlot candidate = slots[i];
                if (candidate == null)
                    continue;

                if (candidate.WeaponType != WeaponType.None)
                    continue;

                if (!IsSlotEmpty(candidate))
                    continue;

                slot = candidate;
                return true;
            }
        }

        return false;
    }

    private bool IsSlotEmpty(CoreSlot slot)
    {
        return slot.GetComponentInChildren<DraggableCore>(true) == null;
    }
}