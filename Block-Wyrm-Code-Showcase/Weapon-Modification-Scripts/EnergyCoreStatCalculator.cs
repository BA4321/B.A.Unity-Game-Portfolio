using System.Collections.Generic;
using UnityEngine;

public static class EnergyCoreStatCalculator
{
    /// <summary>
    /// Creates a clean baseline with new WeaponRuntimeData(source),
    /// then applies each core's modifiers in strict slot order (0..3).
    /// </summary>
    public static WeaponRuntimeData CalculateStats(WeaponDefinition source, List<EnergyCoreSO> cores)
    {
        if (source == null) return null;

        // Clean baseline (runtime-modifiable)
        WeaponRuntimeData data = new WeaponRuntimeData(source);

        // Strict slot order: 0..3
        for (int slot = 0; slot <= 3; slot++)
        {
            EnergyCoreSO core = (cores != null && slot < cores.Count) ? cores[slot] : null;
            if (core == null) continue;

            // Optional compatibility gate (WeaponType inferred from source.id).
            // If weapon type can't be inferred -> allow modifiers (treated as compatible).
            if (!IsCoreCompatibleWithWeapon(source, core))
                continue;

            if (core.StatModifiers == null) continue;

            // Apply modifiers in list order (as stored on the core)
            for (int m = 0; m < core.StatModifiers.Count; m++)
            {
                ApplyModifier(ref data, core.StatModifiers[m]);
            }
        }

        // Safety clamps to keep values sane (matching your definition intent)
        data.damage = Mathf.Max(0f, data.damage);
        data.rateOfFire = Mathf.Max(0.01f, data.rateOfFire);
        data.projectileCount = Mathf.Max(1, data.projectileCount);
        data.projectileSpeed = Mathf.Max(0f, data.projectileSpeed);
        data.spreadAngle = Mathf.Clamp(data.spreadAngle, 0f, 45f);
        data.explosionRadius = Mathf.Max(0f, data.explosionRadius);

        return data;
    }

    private static void ApplyModifier(ref WeaponRuntimeData data, StatModifier mod)
    {
        switch (mod.StatType)
        {
            case StatType.Damage:
                data.damage = ApplyFloat(data.damage, mod.Operation, mod.Value);
                break;

            case StatType.RateOfFire:
                data.rateOfFire = ApplyFloat(data.rateOfFire, mod.Operation, mod.Value);
                break;

            case StatType.ProjectileCount:
                data.projectileCount = ApplyInt(data.projectileCount, mod.Operation, mod.Value);
                break;

            case StatType.ProjectileSpeed:
                data.projectileSpeed = ApplyFloat(data.projectileSpeed, mod.Operation, mod.Value);
                break;

            case StatType.SpreadAngle:
                data.spreadAngle = ApplyFloat(data.spreadAngle, mod.Operation, mod.Value);
                break;

            case StatType.ExplosionRadius:
                data.explosionRadius = ApplyFloat(data.explosionRadius, mod.Operation, mod.Value);
                break;
        }
    }

    private static float ApplyFloat(float current, ModifierOperation op, float value)
    {
        return op == ModifierOperation.Add ? (current + value) : (current * value);
    }

    private static int ApplyInt(int current, ModifierOperation op, float value)
    {
        if (op == ModifierOperation.Add)
            return current + Mathf.RoundToInt(value);

        // Multiply
        return Mathf.RoundToInt(current * value);
    }

    private static bool IsCoreCompatibleWithWeapon(WeaponDefinition source, EnergyCoreSO core)
    {
        if (core == null) return false;

        // None means "compatible with anything"
        if (core.compatibleWeaponType == WeaponType.None)
            return true;

        WeaponType weaponType = InferWeaponTypeFromId(source != null ? source.id : null);

        // If we can't infer type, don't block core usage (treat as compatible).
        if (weaponType == WeaponType.None)
            return true;

        return weaponType == core.compatibleWeaponType;
    }

    private static WeaponType InferWeaponTypeFromId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return WeaponType.None;

        string s = id.Trim().ToLowerInvariant();

        if (s.Contains("rifle"))
            return WeaponType.Rifle;

        if (s.Contains("launcher"))
            return WeaponType.Launcher;

        // Treat common rifle-like ids as Rifle (rifle/smg/ar)
        if (s.Contains("shotgun") || s.Contains("smg") || s == "ar" || s.Contains("assault"))
            return WeaponType.Shotgun;

        return WeaponType.None;
    }
}
