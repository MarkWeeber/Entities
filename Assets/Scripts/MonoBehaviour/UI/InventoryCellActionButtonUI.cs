using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System;

public class InventoryCellActionButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text buttonText;

    public void RegisterButtonAction(string actionName, UnityAction someaction)
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        if (button != null)
        {
            if (buttonText != null)
            {
                buttonText.text = actionName;
            }
            button.onClick.AddListener(someaction);
        }
    }
}
