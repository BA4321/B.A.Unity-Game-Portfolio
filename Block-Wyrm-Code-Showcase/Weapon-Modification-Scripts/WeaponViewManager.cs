using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Added for TextMeshPro support

public class WeaponViewManager : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private WeaponDatabase weaponDatabase;

    [Header("Weapon IDs in WeaponDatabase")]
    [SerializeField] private string rifleId = "rifle";
    [SerializeField] private string shotgunId = "shotgun";
    [SerializeField] private string launcherId = "launcher";

    [Header("Weapon Slot Panels (each contains 4 CoreSlot children)")]
    [SerializeField] private CanvasGroup riflePanel;
    [SerializeField] private CanvasGroup shotgunPanel;
    [SerializeField] private CanvasGroup launcherPanel;

    [Tooltip("If true, keep other panels visible but dimmed. If false, hide them.")]
    [SerializeField] private bool dimOthersInsteadOfHide = true;

    [Range(0f, 1f)]
    [SerializeField] private float dimAlpha = 0.25f;

    [Header("Stat Dashboard (TextMeshPro)")]
    //for some reason damage type slider doesn't show, fix it sometime soon
    [SerializeField] private TextMeshProUGUI DamageTypeText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI rateOfFireText;
    [SerializeField] private TextMeshProUGUI projectileCountText;
    [SerializeField] private TextMeshProUGUI projectileSpeedText;
    [SerializeField] private TextMeshProUGUI spreadAngleText;
    [SerializeField] private TextMeshProUGUI explosionRadiusText;
    

    // Optional sliders (leave null if you only use Text)
    [Header("Stat Dashboard (Optional Sliders)")]
    [SerializeField] private Slider damageSlider;
    [SerializeField] private Slider rateOfFireSlider;
    [SerializeField] private Slider projectileCountSlider;
    [SerializeField] private Slider projectileSpeedSlider;
    [SerializeField] private Slider spreadAngleSlider;
    [SerializeField] private Slider explosionRadiusSlider;

    [Header("Defaults")]
    [SerializeField] public WeaponType activeWeapon = WeaponType.Rifle;

    private readonly CoreSlot[] rifleSlots = new CoreSlot[4];
    private readonly CoreSlot[] shotgunSlots = new CoreSlot[4];
    private readonly CoreSlot[] launcherSlots = new CoreSlot[4];

    private void Awake()
    {
        CacheSlotsFromPanels();
    }

    private void OnEnable()
    {
        CoreSlot.OnInventoryChanged += HandleInventoryChanged;
        ApplyWeaponView(activeWeapon);
        RecalculateAndUpdateDashboard();
    }

    private void OnDisable()
    {
        CoreSlot.OnInventoryChanged -= HandleInventoryChanged;
    }

    private void HandleInventoryChanged()
    {
        // Always recalc for the currently active weapon
        RecalculateAndUpdateDashboard();
    }

    // Hook these to your UI buttons
    public void SelectRifle()  => SetActiveWeapon(WeaponType.Rifle);
    public void SelectShotgun()   => SetActiveWeapon(WeaponType.Shotgun);
    public void SelectLauncher() => SetActiveWeapon(WeaponType.Launcher);

    public void SetActiveWeapon(WeaponType weapon)
    {
        if (activeWeapon == weapon)
        {
            // Still refresh dashboard if you want "tap same button to refresh"
            RecalculateAndUpdateDashboard();
            DraggableCore.Deselect();
            return;
        }

        activeWeapon = weapon;
        DraggableCore.Deselect(); 
        ApplyWeaponView(activeWeapon);
        RecalculateAndUpdateDashboard();
    }

    private void ApplyWeaponView(WeaponType weapon)
    {
        SetPanelState(riflePanel, weapon == WeaponType.Rifle);
        SetPanelState(shotgunPanel, weapon == WeaponType.Shotgun);
        SetPanelState(launcherPanel, weapon == WeaponType.Launcher);
    }

    private void SetPanelState(CanvasGroup panel, bool isActive)
    {
        if (panel == null) return;

        if (!dimOthersInsteadOfHide)
        {
            panel.gameObject.SetActive(isActive);
            return;
        }

        // Dim mode (keep visible, only active one is interactable)
        panel.gameObject.SetActive(true);
        panel.alpha = isActive ? 1f : dimAlpha;
        panel.interactable = isActive;
        panel.blocksRaycasts = isActive;
    }

    private void CacheSlotsFromPanels()
    {
        FillSlotsFromPanel(riflePanel, rifleSlots);
        FillSlotsFromPanel(shotgunPanel, shotgunSlots);
        FillSlotsFromPanel(launcherPanel, launcherSlots);
    }

    private void FillSlotsFromPanel(CanvasGroup panel, CoreSlot[] target)
    {
        for (int i = 0; i < target.Length; i++) target[i] = null;
        if (panel == null) return;

        var slots = panel.GetComponentsInChildren<CoreSlot>(true);
        for (int i = 0; i < slots.Length; i++)
        {
            CoreSlot s = slots[i];
            if (s == null) continue;
            if (s.SlotIndex < 0 || s.SlotIndex > 3) continue;

            target[s.SlotIndex] = s;
        }
    }

    private void RecalculateAndUpdateDashboard()
    {
        if (weaponDatabase == null)
        {
            SetDashboardUnavailable("No WeaponDatabase");
            return;
        }

        WeaponDefinition def = weaponDatabase.GetDefinition(GetActiveWeaponId());
        if (def == null)
        {
            SetDashboardUnavailable("WeaponDefinition not found");
            return;
        }

        // Gather 4 cores for active weapon (empty slots => null)
        List<EnergyCoreSO> cores = GatherActiveWeaponCores();

        // Calculate runtime stats (baseline from source + modifiers in slot order)
        WeaponRuntimeData runtime = EnergyCoreStatCalculator.CalculateStats(def, cores);
        if (runtime == null)
        {
            SetDashboardUnavailable("Runtime calc failed");
            return;
        }

        UpdateDashboard(runtime);
    }

    private string GetActiveWeaponId()
    {
        return activeWeapon switch
        {
            WeaponType.Rifle => rifleId,
            WeaponType.Shotgun => shotgunId,
            WeaponType.Launcher => launcherId,
            _ => rifleId
        };
    }

    private List<EnergyCoreSO> GatherActiveWeaponCores()
    {
        CoreSlot[] slots = activeWeapon switch
        {
            WeaponType.Rifle => rifleSlots,
            WeaponType.Shotgun => shotgunSlots,
            WeaponType.Launcher => launcherSlots,
            _ => rifleSlots
        };

        // Always return exactly 4 entries (nulls allowed)
        var cores = new List<EnergyCoreSO>(4);
        for (int i = 0; i < 4; i++)
        {
            EnergyCoreSO core = null;

            CoreSlot slot = (slots != null && i < slots.Length) ? slots[i] : null;
            if (slot != null)
            {
                DraggableCore occupant = slot.GetComponentInChildren<DraggableCore>(false);
                if (occupant != null) core = occupant.Core;
            }

            cores.Add(core); // null if empty
        }
        return cores;
    }

    private void UpdateDashboard(WeaponRuntimeData rt)
    {
        // TextMeshPro objects use the same .text property as legacy text
        if (DamageTypeText != null) DamageTypeText.text = rt.damageType.ToString();
        if (damageText != null) damageText.text = rt.damage.ToString("0.##");
        if (rateOfFireText != null) rateOfFireText.text = rt.rateOfFire.ToString("0.##");
        if (projectileCountText != null) projectileCountText.text = rt.projectileCount.ToString();
        if (projectileSpeedText != null) projectileSpeedText.text = rt.projectileSpeed.ToString("0.##");
        if (spreadAngleText != null) spreadAngleText.text = rt.spreadAngle.ToString("0.##");
        if (explosionRadiusText != null) explosionRadiusText.text = rt.explosionRadius.ToString("0.##");

        // Optional sliders
        if (damageSlider != null) damageSlider.value = rt.damage;
        if (rateOfFireSlider != null) rateOfFireSlider.value = rt.rateOfFire;
        if (projectileCountSlider != null) projectileCountSlider.value = rt.projectileCount;
        if (projectileSpeedSlider != null) projectileSpeedSlider.value = rt.projectileSpeed;
        if (spreadAngleSlider != null) spreadAngleSlider.value = rt.spreadAngle;
        if (explosionRadiusSlider != null) explosionRadiusSlider.value = rt.explosionRadius;
    }

    private void SetDashboardUnavailable(string reason)
{
    if (DamageTypeText != null) DamageTypeText.text = "-";  // capital D
    if (damageText != null) damageText.text = "-";
    if (rateOfFireText != null) rateOfFireText.text = "-";
    if (projectileCountText != null) projectileCountText.text = "-";
    if (projectileSpeedText != null) projectileSpeedText.text = "-";
    if (spreadAngleText != null) spreadAngleText.text = "-";
    if (explosionRadiusText != null) explosionRadiusText.text = "-";
}
}