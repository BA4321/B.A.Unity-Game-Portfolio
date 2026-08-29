using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UICoreLoadoutDiskPersistence : MonoBehaviour
{
    [Serializable]
    private class SaveFile
    {
        public List<SlotRecord> slots = new List<SlotRecord>();
    }

    [Serializable]
    private class SlotRecord
    {
        public string slotKey;
        public string coreSaveId;
    }

    [Header("References")]
    [SerializeField] private UICorePrefabDatabase prefabDatabase;
    [SerializeField] private List<CoreSlot> slots = new List<CoreSlot>();

    [Header("Slot Discovery")]
    [SerializeField] private bool autoFindSlotsIfListEmpty = true;

    [Header("Auto Save / Load")]
    [SerializeField] private bool loadOnAwake = true;
    [SerializeField] private bool saveOnDisable = true;
    [SerializeField] private bool saveOnApplicationPause = true;

    [Header("Bag Slot Lock State Loading")]
    [SerializeField] private bool loadBagSlotLockStatesOnStart = true;

    [Header("Save File")]
    [SerializeField] private string fileName = "ui_core_loadout.json";

    private string SavePath => Path.Combine(Application.persistentDataPath, fileName);

    private void Awake()
    {
        CacheSlotsIfNeeded();

        if (loadOnAwake)
            LoadFromDisk();
    }

    private void Start()
    {
        if (loadBagSlotLockStatesOnStart)
            LoadBagSlotLockStatesFromGameData();
    }

    private void OnDisable()
    {
        if (Application.isPlaying && saveOnDisable)
            SaveToDisk();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && saveOnApplicationPause)
            SaveToDisk();
    }

    /*
    private void OnApplicationQuit()
    {
        if (saveOnApplicationQuit)
            SaveToDisk();
    }
    */

    // ----------------- BAG SLOT LOCK STATE LOADING -----------------

    [ContextMenu("Load Bag Slot Lock States From GameData")]
    public void LoadBagSlotLockStatesFromGameData()
    {
        CacheSlotsIfNeeded();

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("UICoreLoadoutDiskPersistence: No GameManager found. Using CoreSlot Inspector defaults for lock states.", this);
            ApplyBagSlotInspectorDefaultsOnly();
            return;
        }

        ValidateBagSlotUnlockIndexes();

        int requiredCount = GetRequiredBagSlotUnlockStateCount();

        if (requiredCount <= 0)
            return;

        List<bool> defaultsByIndex = BuildBagSlotDefaultUnlockStates(requiredCount);

        /*
         Required behavior:

         If save data exists and contains this slot index:
             use saved value

         Else:
             use Inspector defaultUnlocked value
             initialize/extend save data from defaults
        */
        GameManager.Instance.EnsureBagSlotUnlockDataUsingDefaults(defaultsByIndex);

        ApplyBagSlotUnlockStatesFromGameData();
    }

    private void ApplyBagSlotInspectorDefaultsOnly()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            CoreSlot slot = slots[i];

            if (slot == null)
                continue;

            if (!slot.IsLockableBagSlot)
                continue;

            slot.ApplySavedBagSlotUnlockedState(slot.InspectorDefaultUnlocked);
        }
    }

    private int GetRequiredBagSlotUnlockStateCount()
    {
        int highestIndex = -1;

        for (int i = 0; i < slots.Count; i++)
        {
            CoreSlot slot = slots[i];

            if (slot == null)
                continue;

            if (!slot.IsLockableBagSlot)
                continue;

            if (slot.BagSlotUnlockIndex > highestIndex)
                highestIndex = slot.BagSlotUnlockIndex;
        }

        return highestIndex + 1;
    }

    private List<bool> BuildBagSlotDefaultUnlockStates(int requiredCount)
    {
        List<bool> defaultsByIndex = new List<bool>(requiredCount);

        for (int i = 0; i < requiredCount; i++)
            defaultsByIndex.Add(false);

        for (int i = 0; i < slots.Count; i++)
        {
            CoreSlot slot = slots[i];

            if (slot == null)
                continue;

            if (!slot.IsLockableBagSlot)
                continue;

            int index = slot.BagSlotUnlockIndex;

            if (index < 0 || index >= requiredCount)
                continue;

            defaultsByIndex[index] = slot.InspectorDefaultUnlocked;
        }

        return defaultsByIndex;
    }

    private void ApplyBagSlotUnlockStatesFromGameData()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            CoreSlot slot = slots[i];

            if (slot == null)
                continue;

            if (!slot.IsLockableBagSlot)
                continue;

            bool unlocked;

            if (GameManager.Instance.TryGetBagSlotUnlocked(slot.BagSlotUnlockIndex, out unlocked))
            {
                slot.ApplySavedBagSlotUnlockedState(unlocked);
            }
            else
            {
                // Extra safety fallback.
                slot.ApplySavedBagSlotUnlockedState(slot.InspectorDefaultUnlocked);
            }
        }
    }

    private void ValidateBagSlotUnlockIndexes()
    {
        HashSet<int> usedIndexes = new HashSet<int>();

        for (int i = 0; i < slots.Count; i++)
        {
            CoreSlot slot = slots[i];

            if (slot == null)
                continue;

            if (!slot.IsLockableBagSlot)
                continue;

            int index = slot.BagSlotUnlockIndex;

            if (index < 0)
            {
                Debug.LogWarning($"CoreSlot has invalid bag slot unlock index: {index}.", slot);
                continue;
            }

            if (!usedIndexes.Add(index))
            {
                Debug.LogWarning(
                    $"Duplicate bag slot unlock index detected: {index}. " +
                    "Each lockable bag slot should have a unique BagSlotIndex.",
                    slot
                );
            }
        }
    }

    // ----------------- CORE LOADOUT SAVE / LOAD -----------------

    [ContextMenu("Save To Disk")]
    public void SaveToDisk()
    {
        CacheSlotsIfNeeded();

        if (prefabDatabase == null)
        {
            Debug.LogWarning("UICoreLoadoutDiskPersistence: Prefab database is not assigned.", this);
            return;
        }

        ValidateSlotKeys();

        SaveFile save = new SaveFile();

        for (int i = 0; i < slots.Count; i++)
        {
            CoreSlot slot = slots[i];
            if (slot == null) continue;

            SlotRecord record = new SlotRecord
            {
                slotKey = BuildSlotKey(slot),
                coreSaveId = string.Empty
            };

            DraggableCore core = GetCoreInSlot(slot);
            if (core != null)
                record.coreSaveId = core.SaveId;

            save.slots.Add(record);
        }

        try
        {
            string json = JsonUtility.ToJson(save, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Core loadout saved to: {SavePath}", this);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save core loadout.\n{ex}", this);
        }
    }

    [ContextMenu("Load From Disk")]
    public void LoadFromDisk()
    {
        CacheSlotsIfNeeded();

        if (prefabDatabase == null)
        {
            Debug.LogWarning("UICoreLoadoutDiskPersistence: Prefab database is not assigned.", this);
            return;
        }

        if (!File.Exists(SavePath))
        {
            Debug.Log("No core loadout save file found. Keeping current scene setup.", this);
            return;
        }

        ValidateSlotKeys();

        try
        {
            string json = File.ReadAllText(SavePath);
            SaveFile save = JsonUtility.FromJson<SaveFile>(json);

            if (save == null || save.slots == null)
            {
                Debug.LogWarning("Core loadout save file is empty or invalid.", this);
                return;
            }

            Dictionary<string, CoreSlot> slotLookup = new Dictionary<string, CoreSlot>(StringComparer.Ordinal);

            for (int i = 0; i < slots.Count; i++)
            {
                CoreSlot slot = slots[i];
                if (slot == null) continue;

                string key = BuildSlotKey(slot);

                if (!slotLookup.ContainsKey(key))
                    slotLookup.Add(key, slot);
            }

            ClearAllCurrentCores();

            DraggableCore.Deselect();

            for (int i = 0; i < save.slots.Count; i++)
            {
                SlotRecord record = save.slots[i];

                if (record == null || string.IsNullOrWhiteSpace(record.slotKey))
                    continue;

                if (!slotLookup.TryGetValue(record.slotKey, out CoreSlot targetSlot) || targetSlot == null)
                    continue;

                if (string.IsNullOrWhiteSpace(record.coreSaveId))
                    continue;

                if (!prefabDatabase.TryGetPrefab(record.coreSaveId, out DraggableCore prefab) || prefab == null)
                {
                    Debug.LogWarning($"No prefab found for saved core id '{record.coreSaveId}'.", this);
                    continue;
                }

                DraggableCore instance = Instantiate(prefab, targetSlot.transform);
                instance.SnapToSlot(targetSlot.transform);
            }

            SyncWeaponRuntimeIfPresent();

            Debug.Log($"Core loadout loaded from: {SavePath}", this);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load core loadout.\n{ex}", this);
        }
    }

    [ContextMenu("Delete Save File")]
    public void DeleteSaveFile()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log($"Deleted core loadout save: {SavePath}", this);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete core loadout save.\n{ex}", this);
        }
    }

    // ----------------- SLOT HELPERS -----------------

    private void CacheSlotsIfNeeded()
    {
        if (slots != null && slots.Count > 0)
            return;

        if (!autoFindSlotsIfListEmpty)
            return;

        CoreSlot[] found = GetComponentsInChildren<CoreSlot>(true);
        slots = new List<CoreSlot>(found);
    }

    private void ValidateSlotKeys()
    {
        HashSet<string> used = new HashSet<string>(StringComparer.Ordinal);

        for (int i = 0; i < slots.Count; i++)
        {
            CoreSlot slot = slots[i];
            if (slot == null) continue;

            string key = BuildSlotKey(slot);

            if (!used.Add(key))
                Debug.LogWarning($"Duplicate CoreSlot key detected: {key}. WeaponType + SlotIndex must be unique.", slot);
        }
    }

    private string BuildSlotKey(CoreSlot slot)
    {
        return $"{slot.WeaponType}:{slot.SlotIndex}";
    }

    private DraggableCore GetCoreInSlot(CoreSlot slot)
    {
        if (slot == null) return null;
        return slot.GetComponentInChildren<DraggableCore>(true);
    }

    private void ClearAllCurrentCores()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            CoreSlot slot = slots[i];
            if (slot == null) continue;

            DraggableCore[] cores = slot.GetComponentsInChildren<DraggableCore>(true);

            for (int j = 0; j < cores.Length; j++)
            {
                if (cores[j] == null) continue;

                cores[j].gameObject.SetActive(false);
                Destroy(cores[j].gameObject);
            }
        }
    }

    private void SyncWeaponRuntimeIfPresent()
    {
        if (UIWeaponLoadoutRuntime.Instance == null)
            return;

        for (int i = 0; i < slots.Count; i++)
        {
            CoreSlot slot = slots[i];
            if (slot == null) continue;

            UIWeaponLoadoutRuntime.Instance.SyncFromSlot(slot);
        }
    }
}