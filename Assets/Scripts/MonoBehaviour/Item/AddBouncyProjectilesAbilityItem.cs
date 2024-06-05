using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AddBouncyProjectilesAbilityItem : MonoBehaviour, IItem
{
    [SerializeField] private string itemName = "Bouncy Projectiles";
    [SerializeField] private Sprite image;
    public string ItemName { get => itemName; }
    public Sprite Image { get => image; }
    private IItemAction[] itemActions;
    public IItemAction[] ItemActions { get => itemActions; }

    private bool actionsAdded;
    private void Awake()
    {
        if (!actionsAdded)
        {
            InitializeActions();
        }
    }

    public void InitializeActions()
    {
        itemActions = new IItemAction[1];
        itemActions[0] = new ItemAddBouncyProjectilesAction("Add bouncy projectiles");
        actionsAdded = true;
    }
}
