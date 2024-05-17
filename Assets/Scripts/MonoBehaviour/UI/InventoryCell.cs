using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using UnityEngine.InputSystem;

public class InventoryCell : MonoBehaviour
{
    [SerializeField] private Transform actionsPanel;
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Image itemImage;
    private bool contained = false;
    public bool Contained { get => contained; }
    private IItem item;
    public IItem Item { get => item; }

    [Inject] private Controls controls;

    private void Awake()
    {
        controls.Player.InventoryToggle.performed += HideActionsPanel;
    }

    public void ContainItem(IItem item)
    {
        this.item = item;
        contained = true;
        itemText.text = item.ItemName;
        itemImage.sprite = item.Image;
        RegisterItemActions();
    }

    public void DisposeItemInCell()
    {
        contained = false;
        var buttons = actionsPanel.GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            Destroy(button);
        }
        item = null;
        actionsPanel.gameObject.SetActive(false);
    }

    private void RegisterItemActions()
    {
        foreach (var itemAction in item.ItemActions)
        {
            var instantiatedGameObject = Instantiate(buttonPrefab, actionsPanel);
            InventoryCellActionButtonUI actionButton = instantiatedGameObject.GetComponent<InventoryCellActionButtonUI>();
            if (actionButton != null)
            {
                actionButton.RegisterButtonAction(itemAction.ActionName, itemAction.ActivateAction);
            }
        }
    }

    public void ToggleShowItemActions()
    {
        bool currentActiveState = actionsPanel.gameObject.activeSelf;
        actionsPanel.gameObject.SetActive(!currentActiveState);
    }

    private void HideActionsPanel(InputAction.CallbackContext context)
    {
        actionsPanel.gameObject.SetActive(false);
    }
}
