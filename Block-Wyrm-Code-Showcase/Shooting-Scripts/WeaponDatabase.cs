using System;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    Normal,
    Fire,
    Electric,
    Explosive
}

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "MobileShooter2D/Weapon Database", order = 0)]
public class WeaponDatabase : ScriptableObject
{
    [Tooltip("Fill this with exactly 4 guns (or more later).")]
    public List<WeaponDefinition> weapons = new List<WeaponDefinition>(4);

    public WeaponDefinition GetDefinition(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;

        for (int i = 0; i < weapons.Count; i++)
        {
            var w = weapons[i];
            if (w != null && string.Equals(w.id, id, StringComparison.OrdinalIgnoreCase))
                return w;
        }
        return null;
    }

    /// <summary>
    /// Returns a runtime-modifiable copy of the weapon stats (safe to change during gameplay).
    /// </summary>
    public WeaponRuntimeData CreateRuntimeData(string id)
    {
        var def = GetDefinition(id);
        if (def == null) return null;
        return new WeaponRuntimeData(def);
    }
}

[Serializable]
public class WeaponDefinition
{
    [Header("Identity")]
    [Tooltip("Unique string id, e.g. 'rifle', 'pickle', 'shotgun', 'launcher'.")]
    public string id;

    [Header("Visuals")]
    public Sprite weaponSprite;
    [Tooltip("Optional muzzle flash / muzzle VFX prefab.")]
    public GameObject muzzleEffectPrefab;

    [Header("Audio")]
    public AudioClip shootSfx;

    [Header("Combat Stats")]
    [Min(1f)] public float damage = 1f;
    public DamageType damageType = DamageType.Normal;

    [Tooltip("Shots per second. Example: 10 = 10 bullets/second.")]
    [Range(0.5f, 20f)] public float rateOfFire = 5f;

    [Tooltip("How many projectiles are spawned per shot (shotgun pellet count, multi-shot, etc.).")]
    [Min(1)] public int projectileCount = 1;

    [Tooltip("Projectile speed units/sec (Rigidbody2D velocity magnitude or manual movement).")]
    [Min(1f)] public float projectileSpeed = 20f;

    [Header("Accuracy")]
    [Tooltip("Maximum angle deviation (in degrees) from the intended direction. 0 = perfectly accurate.")]
    [Range(0f, 45f)] public float spreadAngle = 0f;

    [Header("Projectile")]
    [Tooltip("Which projectile/bullet prefab this weapon uses.")]
    public GameObject bulletPrefab;

    [Header("Explosion (Optional)")]
    public bool hasExplosion = false;

    [Tooltip("Only used if hasExplosion is true.")]
    [Min(0f)] public float explosionRadius = 0f;
}

/// <summary>
/// Runtime copy of WeaponDefinition (modify freely during gameplay without changing the asset).
/// </summary>
[Serializable]
public class WeaponRuntimeData
{
    [NonSerialized] public WeaponDefinition source;

    public string id;
    public Sprite weaponSprite;
    public GameObject muzzleEffectPrefab;

    public AudioClip shootSfx;

    public float damage;
    public DamageType damageType;
    public float rateOfFire;
    public int projectileCount;
    public float projectileSpeed;

    public float spreadAngle;

    public GameObject bulletPrefab;

    public bool hasExplosion;
    public float explosionRadius;

    public WeaponRuntimeData(WeaponDefinition def)
    {
        source = def;

        id = def.id;
        weaponSprite = def.weaponSprite;
        muzzleEffectPrefab = def.muzzleEffectPrefab;

        shootSfx = def.shootSfx;

        damage = def.damage;
        damageType = def.damageType;
        rateOfFire = def.rateOfFire;
        projectileCount = Mathf.Max(1, def.projectileCount);
        projectileSpeed = def.projectileSpeed;

        spreadAngle = def.spreadAngle;

        bulletPrefab = def.bulletPrefab;

        hasExplosion = def.hasExplosion;
        explosionRadius = def.explosionRadius;
    }

    public void ResetToSource()
    {
        if (source == null) return;
        var def = source;

        weaponSprite = def.weaponSprite;
        muzzleEffectPrefab = def.muzzleEffectPrefab;

        shootSfx = def.shootSfx;

        damage = def.damage;
        damageType = def.damageType;
        rateOfFire = def.rateOfFire;
        projectileCount = Mathf.Max(1, def.projectileCount);
        projectileSpeed = def.projectileSpeed;

        spreadAngle = def.spreadAngle;

        bulletPrefab = def.bulletPrefab;

        hasExplosion = def.hasExplosion;
        explosionRadius = def.explosionRadius;
    }
}