using Unity.Entities;

public class ItemData : IComponentData
{
    public IItem Item;
    public Entity TargetEntity;
    public ItemData()
    { }
}