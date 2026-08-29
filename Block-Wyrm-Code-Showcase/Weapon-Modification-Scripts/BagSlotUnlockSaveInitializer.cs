using System.Collections.Generic;
using UnityEngine;

public class BagSlotUnlockSaveInitializer : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private bool autoFindInChildren = true;
    [SerializeField] private List<CoreSlot> lockableBagSlots = new List<CoreSlot>();

    private void Awake()
    {
        if (autoFindInChildren || lockableBagSlots.Count == 0)
            AutoFindLockableBagSlots();
    }

    private void Start()
    {
        InitializeAndApplySaveData();
    }

    [ContextMenu("Auto Find Lockable Bag Slots")]
    private void AutoFindLockableBagSlots()
    {
        lockableBagSlots.Clear();

        CoreSlot[] allSlots = GetComponentsInChildren<CoreSlot>(true);

        for (int i = 0; i < allSlots.Length; i++)
        {
            CoreSlot slot = allSlots[i];

            if (slot != null && slot.IsLockableBagSlot)
                lockableBagSlots.Add(slot);
        }

        lockableBagSlots.Sort((a, b) =>
            a.BagSlotUnlockIndex.CompareTo(b.BagSlotUnlockIndex));
    }

    public void InitializeAndApplySaveData()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[BagSlotUnlockSaveInitializer] No GameManager found.");
            return;
        }

        int requiredCount = GetRequiredSaveListCount();

        if (requiredCount <= 0)
            return;

        List<bool> inspectorDefaultsByIndex = BuildInspectorDefaultsList(requiredCount);

        // Fresh save:
        // Creates save data from Inspector defaults.
        //
        // Existing save:
        // Keeps saved values.
        GameManager.Instance.EnsureBagSlotUnlockDataUsingDefaults(inspectorDefaultsByIndex);

        ApplySavedStatesToSlots();
    }

    private int GetRequiredSaveListCount()
    {
        int highestIndex = -1;

        for (int i = 0; i < lockableBagSlots.Count; i++)
        {
            CoreSlot slot = lockableBagSlots[i];

            if (slot == null)
                continue;

            if (slot.BagSlotUnlockIndex > highestIndex)
                highestIndex = slot.BagSlotUnlockIndex;
        }

        return highestIndex + 1;
    }

    private List<bool> BuildInspectorDefaultsList(int requiredCount)
    {
        List<bool> defaults = new List<bool>(requiredCount);

        for (int i = 0; i < requiredCount; i++)
            defaults.Add(false);

        bool[] indexUsed = new bool[requiredCount];

        for (int i = 0; i < lockableBagSlots.Count; i++)
        {
            CoreSlot slot = lockableBagSlots[i];

            if (slot == null)
                continue;

            int index = slot.BagSlotUnlockIndex;

            if (index < 0 || index >= requiredCount)
                continue;

            if (indexUsed[index])
            {
                Debug.LogWarning(
                    $"[BagSlotUnlockSaveInitializer] Duplicate bag slot unlock index found: {index}. " +
                    "Each lockable bag slot should have a unique SlotIndex."
                );
            }

            indexUsed[index] = true;
            defaults[index] = slot.InspectorDefaultUnlocked;
        }

        return defaults;
    }

    private void ApplySavedStatesToSlots()
    {
        for (int i = 0; i < lockableBagSlots.Count; i++)
        {
            CoreSlot slot = lockableBagSlots[i];

            if (slot == null)
                continue;

            bool unlocked;

            if (GameManager.Instance.TryGetBagSlotUnlocked(slot.BagSlotUnlockIndex, out unlocked))
            {
                slot.ApplySavedBagSlotUnlockedState(unlocked);
            }
            else
            {
                // Fallback safety.
                slot.ApplySavedBagSlotUnlockedState(slot.InspectorDefaultUnlocked);
            }
        }
    }
}