using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryCell : MonoBehaviour
{
    [SerializeField] private Transform actionsPanel;
    [SerializeField] private GameObject ButtonPrefab;
    private bool contained = false;
    public bool Contained { get => contained; }
    private IItem item;
    public IItem Item { get => item; }

    public void ContainItem(IItem item)
    {
        this.item = item;
        contained = true;
        RegisterItemActions();
    }

    private void RegisterItemActions()
    {
        if (item.ItemActions.Length < 1)
        {
            Debug.Log("Missing Item Actions");
            return;
        }
        foreach (var itemAction in item.ItemActions)
        {
            var instantiatedGameObject = Instantiate(ButtonPrefab, actionsPanel);
            InventoryCellActionButtonUI actionButton = instantiatedGameObject.GetComponent<InventoryCellActionButtonUI>();
            if (actionButton != null)
            {
                actionButton.RegisterItemAction(itemAction);
            }
        }
    }

}
