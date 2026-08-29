using System.Collections.Generic;
using UnityEngine;

public class UIWeaponLoadoutRuntime : MonoBehaviour
{
    public static UIWeaponLoadoutRuntime Instance { get; private set; }

    [SerializeField] private EnergyCoreSO[] rifle = new EnergyCoreSO[4];
    [SerializeField] private EnergyCoreSO[] shotgun = new EnergyCoreSO[4];
    [SerializeField] private EnergyCoreSO[] launcher = new EnergyCoreSO[4];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("UIWeaponLoadoutRuntime");
        go.AddComponent<UIWeaponLoadoutRuntime>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureArrays();
    }

    private void EnsureArrays()
    {
        if (rifle == null || rifle.Length != 4) rifle = new EnergyCoreSO[4];
        if (shotgun == null || shotgun.Length != 4) shotgun = new EnergyCoreSO[4];
        if (launcher == null || launcher.Length != 4) launcher = new EnergyCoreSO[4];
    }

    private EnergyCoreSO[] GetArray(WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Rifle => rifle,
            WeaponType.Shotgun => shotgun,
            WeaponType.Launcher => launcher,
            _ => null
        };
    }

    public void SetCore(WeaponType weaponType, int slotIndex, EnergyCoreSO core)
    {
        EnsureArrays();

        var arr = GetArray(weaponType);
        if (arr == null) return;
        if (slotIndex < 0 || slotIndex > 3) return;

        arr[slotIndex] = core;
    }

    public List<EnergyCoreSO> GetCoresList(WeaponType weaponType)
    {
        EnsureArrays();

        var arr = GetArray(weaponType);
        if (arr == null)
            return new List<EnergyCoreSO>(4) { null, null, null, null };

        return new List<EnergyCoreSO>(4) { arr[0], arr[1], arr[2], arr[3] };
    }

    /// <summary>
    /// Reads what's currently inside a weapon slot and saves it into the runtime loadout.
    /// Bag slots are ignored.
    /// </summary>
    public void SyncFromSlot(CoreSlot slot)
    {
        if (slot == null) return;
        if (slot.WeaponType == WeaponType.None) return; // Bag ignored

        DraggableCore occ = slot.GetComponentInChildren<DraggableCore>(true);
        SetCore(slot.WeaponType, slot.SlotIndex, occ != null ? occ.Core : null);
    }
}