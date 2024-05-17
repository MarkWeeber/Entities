using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
using UnityEngine.InputSystem;

public class PanelVisibilityToggleOnClickUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform targetPanel;

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
        if (pointerOver)
        {
            targetPanel.gameObject.SetActive(true);
        }
        else
        {
            targetPanel.gameObject.SetActive(false);
        }
    }

    private void HideTargetPanel(InputAction.CallbackContext context)
    {
        targetPanel.gameObject.SetActive(false);
    }
}
