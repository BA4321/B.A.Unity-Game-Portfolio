using UnityEngine;
using UnityEngine.Events;

public class UICoreShopPurchaseButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UICoreShopPurchaseService purchaseService;

    [Header("Selection")]
    [SerializeField] private DraggableCore selectedCoreDisplay;

    [Header("Events")]
    [SerializeField] private UnityEvent onPurchaseSucceeded;
    [SerializeField] private UnityEvent onDeniedNoSpace;
    [SerializeField] private UnityEvent onDeniedNotEnoughMoney;
    [SerializeField] private UnityEvent onDeniedNotEnoughGems;
    [SerializeField] private UnityEvent onDeniedBusyDragging;
    [SerializeField] private UnityEvent onDeniedNotShopSelection;
    [SerializeField] private UnityEvent onDeniedOther;

    private void OnEnable()
    {
        DraggableCore.OnCoreSelected += HandleCoreSelected;
        selectedCoreDisplay = DraggableCore.CurrentSelected;
    }

    private void OnDisable()
    {
        DraggableCore.OnCoreSelected -= HandleCoreSelected;
    }

    private void HandleCoreSelected(EnergyCoreSO _)
    {
        if (DraggableCore.CurrentSelected != null)
            selectedCoreDisplay = DraggableCore.CurrentSelected;
    }

    public void PurchaseAssignedCore()
    {
        if (purchaseService == null)
        {
            Debug.LogWarning($"{name}: Purchase service is missing.", this);
            onDeniedOther?.Invoke();
            return;
        }

        if (selectedCoreDisplay == null)
            selectedCoreDisplay = DraggableCore.CurrentSelected;

        CoreShopPurchaseResult result = purchaseService.TryPurchase(selectedCoreDisplay);

        switch (result)
        {
            case CoreShopPurchaseResult.Success:
                onPurchaseSucceeded?.Invoke();
                break;

            case CoreShopPurchaseResult.NoInventorySpace:
                onDeniedNoSpace?.Invoke();
                break;

            case CoreShopPurchaseResult.NotEnoughMoney:
                onDeniedNotEnoughMoney?.Invoke();
                break;

            case CoreShopPurchaseResult.NotEnoughGems:
                onDeniedNotEnoughGems?.Invoke();
                break;

            case CoreShopPurchaseResult.BusyDragging:
                onDeniedBusyDragging?.Invoke();
                break;

            case CoreShopPurchaseResult.NotShopSelection:
                onDeniedNotShopSelection?.Invoke();
                break;

            default:
                onDeniedOther?.Invoke();
                break;
        }
    }
}