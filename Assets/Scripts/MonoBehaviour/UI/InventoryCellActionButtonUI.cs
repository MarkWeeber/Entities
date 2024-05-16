using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCellActionButtonUI : MonoBehaviour
{
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void RegisterItemAction(IItemAction itemAction)
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        if (button != null)
        {
            button.name = itemAction.ActionName;
            button.onClick.AddListener(itemAction.ActivateAction);
        }
    }
}
