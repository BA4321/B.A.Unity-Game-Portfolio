using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UICoreShopPurchaseService : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UICorePrefabRegistry prefabRegistry;

    [Header("Inventory Slots (Bag Slots Only)")]
    [SerializeField] private List<CoreSlot> inventorySlots = new List<CoreSlot>();
    [SerializeField] private bool autoFindSlotsIfListEmpty = true;

    [Header("Events")]
    [Tooltip("Useful for SaveToDisk() or refresh UI after a successful purchase.")]
    [SerializeField] private UnityEvent onPurchaseSucceeded;

    public IReadOnlyList<CoreSlot> InventorySlots => inventorySlots;

    private void Awake()
    {
        CacheSlotsIfNeeded();
    }

    [ContextMenu("Refresh Inventory Slots From Children")]
    public void RefreshInventorySlotsFromChildren()
    {
        CoreSlot[] found = GetComponentsInChildren<CoreSlot>(true);

        inventorySlots.Clear();

        for (int i = 0; i < found.Length; i++)
        {
            CoreSlot slot = found[i];

            if (slot == null)
                continue;

            // Shop purchase service should only know about bag slots.
            if (slot.WeaponType != WeaponType.None)
                continue;

            inventorySlots.Add(slot);
        }
    }

    public bool HasEmptyInventorySlot()
    {
        CacheSlotsIfNeeded();

        // This now means:
        // "Has empty unlocked inventory slot."
        return TryFindFirstEmptyUnlockedInventorySlot(out _);
    }

    public CoreShopPurchaseResult CanPurchase(DraggableCore selectedDisplayCore)
    {
        return CanPurchase(selectedDisplayCore, out _);
    }

    public CoreShopPurchaseResult CanPurchase(DraggableCore selectedDisplayCore, out CoreSlot suggestedSlot)
    {
        CacheSlotsIfNeeded();
        suggestedSlot = null;

        if (selectedDisplayCore == null)
            return CoreShopPurchaseResult.InvalidPrefab;

        if (selectedDisplayCore.Core == null)
            return CoreShopPurchaseResult.InvalidCore;

        if (!selectedDisplayCore.IsShopOnly)
            return CoreShopPurchaseResult.NotShopSelection;

        if (prefabRegistry == null)
            return CoreShopPurchaseResult.MissingPrefabRegistry;

        if (!prefabRegistry.TryGetPrefab(selectedDisplayCore.Core, out DraggableCore cleanPrefab) || cleanPrefab == null)
            return CoreShopPurchaseResult.MissingCorePrefab;

        if (GameManager.Instance == null)
            return CoreShopPurchaseResult.MissingGameManager;

        if (DraggableCore.CurrentDragging != null)
            return CoreShopPurchaseResult.BusyDragging;

        // Important:
        // This now only finds empty UNLOCKED bag slots.
        if (!TryFindFirstEmptyUnlockedInventorySlot(out suggestedSlot))
            return CoreShopPurchaseResult.NoInventorySpace;

        int moneyCost = Mathf.Max(0, cleanPrefab.MoneyCost);
        int gemCost = Mathf.Max(0, cleanPrefab.GemCost);

        if (moneyCost > 0 && gemCost > 0)
            return CoreShopPurchaseResult.MixedCurrencyNotSupported;

        return CoreShopPurchaseResult.Success;
    }

    public CoreShopPurchaseResult TryPurchase(DraggableCore selectedDisplayCore)
    {
        return TryPurchase(selectedDisplayCore, out _, out _);
    }

    public CoreShopPurchaseResult TryPurchase(
        DraggableCore selectedDisplayCore,
        out DraggableCore createdCore,
        out CoreSlot placedSlot)
    {
        createdCore = null;
        placedSlot = null;

        CoreShopPurchaseResult canPurchase = CanPurchase(selectedDisplayCore, out placedSlot);

        if (canPurchase != CoreShopPurchaseResult.Success)
            return canPurchase;

        if (!prefabRegistry.TryGetPrefab(selectedDisplayCore.Core, out DraggableCore cleanPrefab) || cleanPrefab == null)
            return CoreShopPurchaseResult.MissingCorePrefab;

        int moneyCost = Mathf.Max(0, cleanPrefab.MoneyCost);
        int gemCost = Mathf.Max(0, cleanPrefab.GemCost);

        bool paid = false;

        if (moneyCost > 0)
        {
            paid = GameManager.Instance.TryPurchaseWithMoney(moneyCost);

            if (!paid)
                return CoreShopPurchaseResult.NotEnoughMoney;
        }
        else if (gemCost > 0)
        {
            paid = GameManager.Instance.TryPurchaseWithGems(gemCost);

            if (!paid)
                return CoreShopPurchaseResult.NotEnoughGems;
        }
        else
        {
            paid = true;
        }

        createdCore = Instantiate(cleanPrefab, placedSlot.transform);
        createdCore.SnapToSlot(placedSlot.transform);

        onPurchaseSucceeded?.Invoke();

        return CoreShopPurchaseResult.Success;
    }

    private void CacheSlotsIfNeeded()
    {
        if (inventorySlots != null && inventorySlots.Count > 0)
            return;

        if (!autoFindSlotsIfListEmpty)
            return;

        RefreshInventorySlotsFromChildren();
    }

    private bool TryFindFirstEmptyUnlockedInventorySlot(out CoreSlot slot)
    {
        slot = null;

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            CoreSlot candidate = inventorySlots[i];

            if (candidate == null)
                continue;

            // Extra safety.
            // You said weapon slots are already not added to this list,
            // but keeping this check is still good protection.
            if (candidate.WeaponType != WeaponType.None)
                continue;

            // New lock rule:
            // Purchases cannot go into locked bag slots.
            if (!candidate.IsUnlocked)
                continue;

            if (!IsSlotEmpty(candidate))
                continue;

            slot = candidate;
            return true;
        }

        return false;
    }

    private bool IsSlotEmpty(CoreSlot slot)
    {
        if (slot == null)
            return false;

        return slot.GetComponentInChildren<DraggableCore>(true) == null;
    }
}