using Unity.Entities;

public class ItemData : IComponentData
{
    public IItem item;
    public ItemData()
    { }
}