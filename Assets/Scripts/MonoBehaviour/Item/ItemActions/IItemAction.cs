using Unity.Entities;

public interface IItemAction
{
    string ActionName { get; set; }
    Entity Entity { get; set; }
    EntityManager EntityManager { get; set; }
    void ActivateAction();
}