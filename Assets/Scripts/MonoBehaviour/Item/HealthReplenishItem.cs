using Unity.Entities;
using UnityEngine;

public class HealthReplenishItem : MonoBehaviour, IItem
{
    [SerializeField] private float healthReplenishAmount;
    private IItemAction[] itemActions;
    public IItemAction[] ItemActions { get => itemActions; set => itemActions = value; }

    private void Awake()
    {
        InitializeActions();
    }

    public void InitializeActions()
    {
        itemActions = new IItemAction[1];
        itemActions[0] = new ItemHealAction("Heal", healthReplenishAmount);
        Debug.Log("Action added");
    }
}
