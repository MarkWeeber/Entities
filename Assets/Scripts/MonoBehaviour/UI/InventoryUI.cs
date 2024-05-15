using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Zenject;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private UnityEvent OnInventoryOpen;
    [SerializeField] private UnityEvent OnInventoryClose;

    private Controls controls;
    private bool inventoryOpen;

    [Inject]
    private void Init(Controls controls)
    {
        this.controls = controls;
        controls.Player.InventoryToggle.performed += OnPress;
    }

    private void OnDestroy()
    {
        if (controls != null)
        {
            controls.Player.InventoryToggle.performed -= OnPress;
        }
    }

    private void OnPress(InputAction.CallbackContext context)
    {
        if (inventoryOpen)
        {
            OnInventoryClose?.Invoke();
            inventoryOpen = false;
        }
        else
        {
            OnInventoryOpen?.Invoke();
            inventoryOpen = true;
        }
    }
}
