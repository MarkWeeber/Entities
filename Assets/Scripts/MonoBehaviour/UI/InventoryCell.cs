using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InventoryCell : MonoBehaviour
{
    [SerializeField] private Transform actionsPanel;
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Image itemImage;
    private bool contained = false;
    private List<GameObject> activeButtons = new();
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
        itemImage.enabled = true;
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
        itemText.text = "Empty";
        foreach (var activeButton in activeButtons)
        {
            Destroy(activeButton);
        }
        activeButtons.Clear();
        itemImage.enabled = false;
        itemImage.sprite = null;
        actionsPanel.gameObject.SetActive(false);
    }

    private void RegisterItemActions()
    {
        for (int i = 0; i < item.ItemActions.Length; i++)
        {
            var itemAction = item.ItemActions[i];
            var instantiatedButton = Instantiate(buttonPrefab, actionsPanel);
            InventoryCellActionButtonUI actionButton = instantiatedButton.GetComponent<InventoryCellActionButtonUI>();
            activeButtons.Add(instantiatedButton.gameObject);
            if (actionButton != null)
            {
                actionButton.RegisterButtonAction(itemAction.ActionName, itemAction.ActivateAction);
                if (i == item.ItemActions.Length - 1)
                {
                    instantiatedButton = Instantiate(buttonPrefab, actionsPanel);
                    actionButton = instantiatedButton.GetComponent<InventoryCellActionButtonUI>();
                    activeButtons.Add(instantiatedButton.gameObject);
                    actionButton.RegisterButtonAction("Drop Item", DisposeItemInCell);
                }
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
