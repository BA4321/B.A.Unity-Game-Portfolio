using System;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    None,
    Rifle,
    Shotgun,
    Launcher
}

public enum CoreRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum StatType
{
    Damage,
    RateOfFire,
    ProjectileCount,
    ProjectileSpeed,
    SpreadAngle,
    ExplosionRadius
}

public enum ModifierOperation
{
    Add,
    Multiply
}

[Serializable]
public struct StatModifier
{
    public StatType StatType;
    public ModifierOperation Operation;
    public float Value;
}

[CreateAssetMenu(fileName = "EnergyCore", menuName = "MobileShooter2D/Energy Core", order = 10)]
public class EnergyCoreSO : ScriptableObject
{
    [Header("Core Info")]
    public string Name;
    public Sprite Icon;
    public CoreRarity Rarity = CoreRarity.Common;

    [Header("Compatibility")]
    public WeaponType compatibleWeaponType = WeaponType.None;

    [Header("Stat Modifiers")]
    public List<StatModifier> StatModifiers = new List<StatModifier>();
}
