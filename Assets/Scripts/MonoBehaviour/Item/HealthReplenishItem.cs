using UnityEngine;

public class HealthReplenishItem : MonoBehaviour, IItem
{
    [SerializeField] private float healthReplenishAmount;
    [SerializeField] private string itemName = "Health";
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
        itemActions[0] = new ItemHealAction("Heal", healthReplenishAmount);
        actionsAdded = true;
    }
}
