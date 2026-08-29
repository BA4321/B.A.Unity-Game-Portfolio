using UnityEngine;
using System.Collections.Generic;

public class JoystickAimShooter2D_Mobile : MonoBehaviour
{
    [Header("Weapon Source")]
    [SerializeField] private WeaponDatabase weaponDatabase;
    [SerializeField] private string weaponId = "rifle";

    [Header("Weapon UI Panels (each has 4 CoreSlot children)")]
    [SerializeField] private Transform riflePanel;
    [SerializeField] private Transform shotgunPanel;
    [SerializeField] private Transform launcherPanel;


    
    [Header("MuzzleFlashObject")]
    [SerializeField] private MuzzleFlash muzzleFlash;

    [Header("Aim Joystick")]
    [SerializeField] private SimpleJoystick aimJoystick;

    [Header("Spawning")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;

    [Header("Aim")]
    [SerializeField, Range(0.01f, 1f)] private float aimDeadzone = 0.2f;

    [Header("Weapon Visuals")]
    [Tooltip("Rotate this pivot to aim (usually your weapon/arm pivot).")]
    [SerializeField] private Transform weaponPivot;

    [Header("Explosion VFX Pool")]
    [SerializeField] private ExplosionVfxPool2D explosionVfxPool;

    [Tooltip("Final VFX scale = explosion radius * this value.")]
    [SerializeField, Min(0.01f)] private float explosionVfxScalePerRadius = 1f;

    [Header("Body Parts")]
    [SerializeField] private Transform headPivot;

    [Tooltip("SpriteRenderer that displays the equipped weapon sprite.")]
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;

    [Header("Sprite Flip (Character)")]
    [SerializeField] private Transform spriteToFlip;
    [SerializeField] private bool enableFlip = true;

    [Header("Weapon Switching")]
    [SerializeField] private bool autoSelectFirstWeaponOnStart = true;
    public int CurrentWeaponIndex { get; private set; } = -1;

    private WeaponRuntimeData _weapon;
    private float _damage = 1f;
    private float _rateOfFire = 5f;
    private int _projectileCount = 1;
    private float _spreadAngle = 0f;
    private bool _hasExplosion = false;
    private float _explosionRadius = 0f;
    private DamageType _damageType = DamageType.Normal;

    private float _nextShotTime;

    public int FacingSign { get; private set; } = 1;
    public float LastAimX { get; private set; } = 1f;

    private float _initialAbsScaleX = 1f;

    private void Awake()
    {
        if (explosionVfxPool == null)
            explosionVfxPool = GetComponent<ExplosionVfxPool2D>();
        if (spriteToFlip == null)
            spriteToFlip = transform;

        _initialAbsScaleX = Mathf.Abs(spriteToFlip.localScale.x);
        if (_initialAbsScaleX < 0.0001f) _initialAbsScaleX = 1f;

        if (weaponDatabase != null && weaponDatabase.weapons != null && weaponDatabase.weapons.Count > 0)
        {
            if (autoSelectFirstWeaponOnStart)
                EquipFirstValidWeapon();
            else if (!TryEquipWeaponById(weaponId))
                EquipFirstValidWeapon();
        }
    }

    private void Update()
    {
        if (aimJoystick == null) return;

        Vector2 aim = aimJoystick.Direction;
        float mag = aim.magnitude;
        if (mag < aimDeadzone)
            return;

        Vector2 aimDir = aim / mag;

        LastAimX = aimDir.x;
        if (Mathf.Abs(LastAimX) > 0.001f)
        {
            FacingSign = (LastAimX >= 0f) ? 1 : -1;
            if (enableFlip) FlipCharacterSprite(FacingSign);
        }

        UpdateWeaponAimVisual(aimDir);
        TryShoot(aimDir);
    }

    private void OnEnable()
    {
        CoreSlot.OnInventoryChanged += RefreshWeaponFromCores;
    }

    private void OnDisable()
    {
        CoreSlot.OnInventoryChanged -= RefreshWeaponFromCores;
    }

    public void NextWeapon()
    {
        FlushCurrentWeaponCoresToRuntime();
        StepWeapon(+1);
    }

    public void PrevWeapon()
    {
        FlushCurrentWeaponCoresToRuntime();
        StepWeapon(-1);
    }

    public bool EquipWeaponById(string id) => TryEquipWeaponById(id);
    public bool EquipWeaponByIndex(int index) => TryEquipWeaponByIndex(index);

    /// <summary>
    /// Saves the current weapon's panel cores into UIWeaponLoadoutRuntime before switching,
    /// so stats are preserved when returning to this weapon or loading into a new scene.
    /// </summary>
    private void FlushCurrentWeaponCoresToRuntime()
    {
        if (UIWeaponLoadoutRuntime.Instance == null) return;

        Transform currentPanel = GetPanelForWeaponId(weaponId);
        if (currentPanel == null) return;

        foreach (var slot in currentPanel.GetComponentsInChildren<CoreSlot>(true))
            UIWeaponLoadoutRuntime.Instance.SyncFromSlot(slot);
    }

    private void FlipCharacterSprite(int facingSign)
    {
        if (spriteToFlip == null) return;
        Vector3 s = spriteToFlip.localScale;
        s.x = _initialAbsScaleX * Mathf.Sign(facingSign);
        if (Mathf.Approximately(s.x, 0f)) s.x = _initialAbsScaleX;
        spriteToFlip.localScale = s;
    }

    private void UpdateWeaponAimVisual(Vector2 aimDir)
    {
        if (weaponPivot == null) return;

        float ang = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        float yRotation = 0f;
        float zRotation;

        if (FacingSign < 0)
        {
            zRotation = -ang + 180f;
        }
        else
        {
            zRotation = ang;
        }

        weaponPivot.localRotation = Quaternion.Euler(0f, yRotation, zRotation);

        if (headPivot != null)
        {
            headPivot.localRotation = Quaternion.Euler(0f, yRotation, zRotation);
        }

        if (weaponSpriteRenderer != null)
        {
            weaponSpriteRenderer.flipY = false;
        }
    }

    private void TryShoot(Vector2 aimDir)
    {
        if (Time.time < _nextShotTime) return;
        if (muzzle == null || bulletPrefab == null) return;

        float secondsPerShot = 1f / Mathf.Max(0.01f, _rateOfFire);
        _nextShotTime = Time.time + secondsPerShot;

        for (int i = 0; i < _projectileCount; i++)
        {
            Vector2 dir = ApplySpread(aimDir, _spreadAngle);

            GameObject b = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);

            muzzleFlash.Play();//For test stuff


            var bullet = b.GetComponent<Bullet2D_Mobile>();
            if (bullet != null)
            {
                bullet.Init(dir, bulletSpeed, _damage, gameObject, _damageType, _hasExplosion, _explosionRadius, explosionVfxPool, explosionVfxScalePerRadius);
            }
            else
            {
                var rb = b.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = dir * bulletSpeed;
                    float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    rb.rotation = ang;
                }
            }
        }
    }

    private Vector2 ApplySpread(Vector2 baseDir, float maxAngleDeg)
    {
        if (maxAngleDeg <= 0.001f) return baseDir;

        float half = maxAngleDeg * 0.5f;
        float angleOffset = Random.Range(-half, half);

        float baseAng = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float finalAng = baseAng + angleOffset;

        float rad = finalAng * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    private void StepWeapon(int dir)
    {
        if (weaponDatabase == null || weaponDatabase.weapons == null) return;
        int count = weaponDatabase.weapons.Count;
        if (count == 0) return;

        if (CurrentWeaponIndex < 0 || CurrentWeaponIndex >= count)
        {
            EquipFirstValidWeapon();
            return;
        }

        int idx = CurrentWeaponIndex;
        for (int tries = 0; tries < count; tries++)
        {
            idx = (idx + dir) % count;
            if (idx < 0) idx += count;

            var def = weaponDatabase.weapons[idx];
            if (def != null && !string.IsNullOrWhiteSpace(def.id))
            {
                ApplyWeapon(def, idx);
                return;
            }
        }
    }

    private void EquipFirstValidWeapon()
    {
        if (weaponDatabase == null || weaponDatabase.weapons == null) return;

        for (int i = 0; i < weaponDatabase.weapons.Count; i++)
        {
            var def = weaponDatabase.weapons[i];
            if (def != null && !string.IsNullOrWhiteSpace(def.id))
            {
                ApplyWeapon(def, i);
                return;
            }
        }

        CurrentWeaponIndex = -1;
    }

    private bool TryEquipWeaponById(string id)
    {
        if (weaponDatabase == null || weaponDatabase.weapons == null) return false;

        for (int i = 0; i < weaponDatabase.weapons.Count; i++)
        {
            var def = weaponDatabase.weapons[i];
            if (def == null) continue;

            if (string.Equals(def.id, id, System.StringComparison.OrdinalIgnoreCase))
            {
                ApplyWeapon(def, i);
                return true;
            }
        }
        return false;
    }

    private bool TryEquipWeaponByIndex(int index)
    {
        if (weaponDatabase == null || weaponDatabase.weapons == null) return false;
        if (weaponDatabase.weapons.Count == 0) return false;
        if (index < 0 || index >= weaponDatabase.weapons.Count) return false;

        var def = weaponDatabase.weapons[index];
        if (def == null || string.IsNullOrWhiteSpace(def.id)) return false;

        ApplyWeapon(def, index);
        RefreshWeaponFromCores();

        return true;
    }

    private void ApplyWeapon(WeaponDefinition def, int index)
    {
        weaponId = def.id;
        CurrentWeaponIndex = index;
        _weapon = new WeaponRuntimeData(def);

        _damage = Mathf.Max(1f, _weapon.damage);
        _damageType = _weapon.damageType;
        _rateOfFire = Mathf.Max(0.67f, _weapon.rateOfFire);
        _projectileCount = Mathf.Max(1, _weapon.projectileCount);
        _spreadAngle = Mathf.Clamp(_weapon.spreadAngle, 0f, 45f);

        _hasExplosion = _weapon.hasExplosion;
        _explosionRadius = _weapon.explosionRadius;

        if (_weapon.bulletPrefab != null)
            bulletPrefab = _weapon.bulletPrefab;

        UpdateWeaponSprite();
        RefreshWeaponFromCores();
    }

    private void UpdateWeaponSprite()
    {
        if (weaponSpriteRenderer == null) return;

        Sprite s = null;
        if (_weapon != null) s = _weapon.weaponSprite;

        weaponSpriteRenderer.sprite = s;
    }

    private void RefreshWeaponFromCores()
    {
        if (weaponDatabase == null) return;

        WeaponDefinition def = weaponDatabase.GetDefinition(weaponId);
        if (def == null) return;

        Transform activePanel = GetPanelForWeaponId(weaponId);
        List<EnergyCoreSO> cores = Gather4CoresFromPanel(activePanel);

        // Persist panel cores into runtime so they survive scene transitions and weapon swaps
        if (activePanel != null && UIWeaponLoadoutRuntime.Instance != null)
        {
            WeaponType wt = InferWeaponTypeFromId(weaponId);
            for (int i = 0; i < cores.Count && i < 4; i++)
                UIWeaponLoadoutRuntime.Instance.SetCore(wt, i, cores[i]);
        }

        WeaponRuntimeData rt = EnergyCoreStatCalculator.CalculateStats(def, cores);
        if (rt == null) return;

        _weapon = rt;

        _damage = Mathf.Max(0f, _weapon.damage);
        _damageType = _weapon.damageType;
        _rateOfFire = Mathf.Max(0.01f, _weapon.rateOfFire);
        _projectileCount = Mathf.Max(1, _weapon.projectileCount);
        _spreadAngle = Mathf.Clamp(_weapon.spreadAngle, 0f, 45f);

        _hasExplosion = _weapon.hasExplosion;
        _explosionRadius = Mathf.Max(0f, _weapon.explosionRadius);

        bulletSpeed = Mathf.Max(0f, _weapon.projectileSpeed);

        if (_weapon.bulletPrefab != null)
            bulletPrefab = _weapon.bulletPrefab;

        UpdateWeaponSprite();
    }

    private Transform GetPanelForWeaponId(string id)
    {
        WeaponType wt = InferWeaponTypeFromId(id);

        return wt switch
        {
            WeaponType.Rifle => riflePanel,
            WeaponType.Shotgun => shotgunPanel,
            WeaponType.Launcher => launcherPanel,
            _ => riflePanel
        };
    }

    private WeaponType InferWeaponTypeFromId(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return WeaponType.None;
        string s = id.Trim().ToLowerInvariant();

        if (s.Contains("rifle")) return WeaponType.Rifle;
        if (s.Contains("launcher")) return WeaponType.Launcher;
        if (s.Contains("shotgun") || s.Contains("smg") || s.Contains("assault") || s == "ar") return WeaponType.Shotgun;

        return WeaponType.None;
    }

    private List<EnergyCoreSO> Gather4CoresFromPanel(Transform panelRoot)
    {
        if (panelRoot == null)
        {
            WeaponType wt = InferWeaponTypeFromId(weaponId);
            if (UIWeaponLoadoutRuntime.Instance != null)
                return UIWeaponLoadoutRuntime.Instance.GetCoresList(wt);

            return new List<EnergyCoreSO>(4) { null, null, null, null };
        }

        var result = new List<EnergyCoreSO>(4) { null, null, null, null };

        CoreSlot[] slots = panelRoot.GetComponentsInChildren<CoreSlot>(true);

        CoreSlot[] indexed = new CoreSlot[4];
        for (int i = 0; i < slots.Length; i++)
        {
            CoreSlot s = slots[i];
            if (s == null) continue;
            if (s.SlotIndex < 0 || s.SlotIndex > 3) continue;

            indexed[s.SlotIndex] = s;
        }

        for (int i = 0; i < 4; i++)
        {
            var slot = indexed[i];
            if (slot == null) { result[i] = null; continue; }

            DraggableCore occ = slot.GetComponentInChildren<DraggableCore>(true);
            result[i] = occ != null ? occ.Core : null;
        }

        return result;
    }
}