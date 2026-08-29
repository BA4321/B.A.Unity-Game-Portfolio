using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Details Box UI")]
    [SerializeField] private GameObject detailsBoxRoot;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Image iconImage;

    [Header("Cost UI")]
    [SerializeField] private GameObject moneyPanelRoot;
    [SerializeField] private GameObject gemPanelRoot;
    [SerializeField] private TextMeshProUGUI moneyCostText;
    [SerializeField] private TextMeshProUGUI gemCostText;

    [Header("Delete Confirmation UI")]
    [SerializeField] private GameObject deleteConfirmPanelRoot;
    [SerializeField] private TextMeshProUGUI deleteNameText;
    [SerializeField] private TextMeshProUGUI deleteRarityText;
    [SerializeField] private TextMeshProUGUI deleteStatsText;
    [SerializeField] private Image deleteIconImage;

    private EnergyCoreSO currentSelectedCore;

    private void OnEnable()
    {
        DraggableCore.OnCoreSelected += HandleCoreSelected;
        RefreshDetails(null);
        RefreshDeleteConfirmation(null);
    }

    private void OnDisable()
    {
        DraggableCore.OnCoreSelected -= HandleCoreSelected;
    }

    private void HandleCoreSelected(EnergyCoreSO core)
    {
        currentSelectedCore = core;
        RefreshDetails(core);

        // If delete panel is already open, keep it in sync too.
        if (deleteConfirmPanelRoot != null && deleteConfirmPanelRoot.activeSelf)
            RefreshDeleteConfirmation(core);
    }

    private void RefreshDetails(EnergyCoreSO core)
    {
        if (detailsBoxRoot != null)
            detailsBoxRoot.SetActive(core != null);

        if (core == null)
        {
            if (nameText != null) nameText.text = "";
            if (rarityText != null) rarityText.text = "";
            if (statsText != null) statsText.text = "";

            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            RefreshCostUI(null);
            return;
        }

        if (nameText != null) nameText.text = core.Name;
        if (rarityText != null) rarityText.text = core.Rarity.ToString();

        if (iconImage != null)
        {
            iconImage.sprite = core.Icon;
            iconImage.enabled = (core.Icon != null);
        }

        if (statsText != null)
            statsText.text = BuildStatsText(core);

        RefreshCostUI(DraggableCore.CurrentSelected);
    }

    private void RefreshCostUI(DraggableCore selectedCore)
    {
        if (selectedCore == null)
        {
            if (moneyPanelRoot != null) moneyPanelRoot.SetActive(false);
            if (gemPanelRoot != null) gemPanelRoot.SetActive(false);
            if (moneyCostText != null) moneyCostText.text = "";
            if (gemCostText != null) gemCostText.text = "";
            return;
        }

        bool hasGemCost = selectedCore.GemCost > 0;
        bool hasMoneyCost = selectedCore.MoneyCost > 0;

        if (hasGemCost)
        {
            if (gemPanelRoot != null) gemPanelRoot.SetActive(true);
            if (moneyPanelRoot != null) moneyPanelRoot.SetActive(false);
            if (gemCostText != null) gemCostText.text = selectedCore.GemCost.ToString();
            if (moneyCostText != null) moneyCostText.text = "";
        }
        else if (hasMoneyCost)
        {
            if (moneyPanelRoot != null) moneyPanelRoot.SetActive(true);
            if (gemPanelRoot != null) gemPanelRoot.SetActive(false);
            if (moneyCostText != null) moneyCostText.text = selectedCore.MoneyCost.ToString();
            if (gemCostText != null) gemCostText.text = "";
        }
        else
        {
            if (moneyPanelRoot != null) moneyPanelRoot.SetActive(false);
            if (gemPanelRoot != null) gemPanelRoot.SetActive(false);
            if (moneyCostText != null) moneyCostText.text = "";
            if (gemCostText != null) gemCostText.text = "";
        }
    }

    private void RefreshDeleteConfirmation(EnergyCoreSO core)
    {
        if (core == null)
        {
            if (deleteNameText != null) deleteNameText.text = "";
            if (deleteRarityText != null) deleteRarityText.text = "";
            if (deleteStatsText != null) deleteStatsText.text = "";

            if (deleteIconImage != null)
            {
                deleteIconImage.sprite = null;
                deleteIconImage.enabled = false;
            }
            return;
        }

        if (deleteNameText != null) deleteNameText.text = core.Name;
        if (deleteRarityText != null) deleteRarityText.text = core.Rarity.ToString();
        if (deleteStatsText != null) deleteStatsText.text = BuildStatsText(core);

        if (deleteIconImage != null)
        {
            deleteIconImage.sprite = core.Icon;
            deleteIconImage.enabled = (core.Icon != null);
        }
    }

    private string BuildStatsText(EnergyCoreSO core)
    {
        if (core.StatModifiers == null || core.StatModifiers.Count == 0)
            return "No modifiers.";

        var sb = new StringBuilder(128);

        for (int i = 0; i < core.StatModifiers.Count; i++)
        {
            var m = core.StatModifiers[i];

            string op = m.Operation == ModifierOperation.Add
                ? (m.Value >= 0 ? "+" : "")  // negative values already have their own "-"
                : "x";
            string valueStr = m.Value.ToString("0.##");

            sb.Append(m.StatType);
            sb.Append(": ");
            sb.Append(op);
            sb.Append(valueStr);

            if (i < core.StatModifiers.Count - 1)
                sb.AppendLine();
        }

        return sb.ToString();
    }

    public void OpenDeleteConfirmation()
    {
        DraggableCore selected = DraggableCore.CurrentSelected;
        if (selected == null || currentSelectedCore == null)
            return;

        RefreshDeleteConfirmation(currentSelectedCore);

        if (deleteConfirmPanelRoot != null)
            deleteConfirmPanelRoot.SetActive(true);
    }

    public void CloseDeleteConfirmation()
    {
        if (deleteConfirmPanelRoot != null)
            deleteConfirmPanelRoot.SetActive(false);
    }

    public void DeleteCore()   //may delete this part, feel like it will fuck up the saving part 
    {
        DraggableCore selected = DraggableCore.CurrentSelected;
        if (selected == null)
        {
            CloseDeleteConfirmation();
            return;
        }

        CoreSlot parentSlot = selected.GetComponentInParent<CoreSlot>();

        if (UIWeaponLoadoutRuntime.Instance != null && parentSlot != null && parentSlot.WeaponType != WeaponType.None)
        {
            UIWeaponLoadoutRuntime.Instance.SetCore(parentSlot.WeaponType, parentSlot.SlotIndex, null);
        }

        Destroy(selected.gameObject);

        currentSelectedCore = null;
        DraggableCore.Select(null);

        if (deleteConfirmPanelRoot != null)
            deleteConfirmPanelRoot.SetActive(false);

        RefreshDeleteConfirmation(null);
        RefreshCostUI(null);

        CoreSlot.OnInventoryChanged?.Invoke();
    }
}