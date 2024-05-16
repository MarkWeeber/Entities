using Unity.Entities;

public class PlayerInventoryData : IComponentData
{
    public InventoryManager InventoryManager;
    public PlayerInventoryData()
    { }
}