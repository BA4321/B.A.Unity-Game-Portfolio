using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UICorePrefabRegistry", menuName = "UI/Core Prefab Registry")]
public class UICorePrefabRegistry : ScriptableObject
{
    [SerializeField] private List<DraggableCore> prefabs = new List<DraggableCore>();

    private Dictionary<EnergyCoreSO, DraggableCore> prefabByCore;

    public bool TryGetPrefab(EnergyCoreSO core, out DraggableCore prefab)
    {
        BuildCacheIfNeeded();

        if (core == null)
        {
            prefab = null;
            return false;
        }

        return prefabByCore.TryGetValue(core, out prefab) && prefab != null;
    }

    private void BuildCacheIfNeeded()
    {
        if (prefabByCore != null)
            return;

        prefabByCore = new Dictionary<EnergyCoreSO, DraggableCore>();

        for (int i = 0; i < prefabs.Count; i++)
        {
            DraggableCore prefab = prefabs[i];

            if (prefab == null)
                continue;

            EnergyCoreSO core = prefab.Core;

            if (core == null)
            {
                Debug.LogWarning(
                    $"UICorePrefabRegistry '{name}' contains prefab '{prefab.name}' with no EnergyCoreSO assigned.",
                    this);
                continue;
            }

            if (!prefabByCore.ContainsKey(core))
            {
                prefabByCore.Add(core, prefab);
            }
            else
            {
                Debug.LogWarning(
                    $"UICorePrefabRegistry '{name}' has multiple prefabs using the same core '{core.name}'. " +
                    $"Only one prefab per core type should exist.",
                    this);
            }
        }
    }

    
    private void OnValidated() // added a "d" at end because it stops me from adding new list items. 16.05.2026 im such a retard for doing something like this 
    {
        prefabByCore = null;

        // Clean nulls automatically
        for (int i = prefabs.Count - 1; i >= 0; i--)
        {
            if (prefabs[i] == null)
                prefabs.RemoveAt(i);
        }
    } 
}