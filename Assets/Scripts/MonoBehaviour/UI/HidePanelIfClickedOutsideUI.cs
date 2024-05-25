using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

public class HidePanelIfClickedOutsideUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool pointerOver;
    [Inject] private Controls controls;

    private void Awake()
    {
        controls.Player.InventoryToggle.performed += HideTargetPanel;
        controls.Player.ClickOnScreen.performed += OnUserClick;
    }

    private void Destroy()
    {
        controls.Player.InventoryToggle.performed -= HideTargetPanel;
        controls.Player.ClickOnScreen.performed -= OnUserClick;
    }

    private void OnDisable()
    {
        pointerOver = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerOver = false;
    }

    private void OnUserClick(InputAction.CallbackContext context)
    {
        if (!pointerOver)
        {
            gameObject.SetActive(false);
        }
    }

    private void HideTargetPanel(InputAction.CallbackContext context)
    {
        gameObject.SetActive(false);
        pointerOver = false;
    }
}
