using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UICorePrefabDatabase", menuName = "UI/Core Prefab Database")]
public class UICorePrefabDatabase : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string saveId;
        public DraggableCore prefab;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    private Dictionary<string, DraggableCore> prefabById;

    public bool TryGetPrefab(string saveId, out DraggableCore prefab)
    {
        BuildCacheIfNeeded();

        if (string.IsNullOrWhiteSpace(saveId))
        {
            prefab = null;
            return false;
        }

        return prefabById.TryGetValue(saveId, out prefab);
    }

    private void BuildCacheIfNeeded()
    {
        if (prefabById != null)
            return;

        prefabById = new Dictionary<string, DraggableCore>(StringComparer.Ordinal);

        for (int i = 0; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            if (entry == null || entry.prefab == null || string.IsNullOrWhiteSpace(entry.saveId))
                continue;

            if (!prefabById.ContainsKey(entry.saveId))
                prefabById.Add(entry.saveId, entry.prefab);
            else
                Debug.LogWarning($"Duplicate saveId in UICorePrefabDatabase: {entry.saveId}", this);
        }
    }

    private void OnValidate()
    {
        prefabById = null;

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i] == null || entries[i].prefab == null)
                continue;

            if (string.IsNullOrWhiteSpace(entries[i].saveId))
                entries[i].saveId = entries[i].prefab.SaveId;
        }
    }
}