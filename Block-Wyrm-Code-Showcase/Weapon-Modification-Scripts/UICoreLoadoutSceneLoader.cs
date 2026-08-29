using System.Collections.Generic;
using UnityEngine;

public class UICoreLoadoutSceneLoader : MonoBehaviour
{
    [SerializeField] private List<UICoreLoadoutDiskPersistence> inventories = new List<UICoreLoadoutDiskPersistence>();

    private void Start()
    {
        for (int i = 0; i < inventories.Count; i++)
        {
            if (inventories[i] == null)
                continue;

            inventories[i].LoadFromDisk();
        }
    }
}