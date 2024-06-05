using Unity.Entities;
using UnityEngine;

public class ItemAddBouncyProjectilesAction : IItemAction
{
    private string actionName;
    public string ActionName { get => actionName; set => actionName = value; }
    private Entity entity;
    public Entity Entity { get => entity; set => entity = value; }
    private EntityManager entityManager;
    public EntityManager EntityManager { get => entityManager; set => entityManager = value; }

    public ItemAddBouncyProjectilesAction(string actionName)
    {
        this.actionName = actionName;
    }

    public void ActivateAction()
    {
        if (entityManager.HasComponent<FireAbilityData>(entity))
        {
            var fireAbilityComponent = entityManager.GetComponentData<FireAbilityData>(entity);
            entityManager.SetComponentData<FireAbilityData>(entity, new FireAbilityData
            {
                Active = fireAbilityComponent.Active,
                FirePortEntity = fireAbilityComponent.FirePortEntity,
                FirePortForwarDirection = fireAbilityComponent.FirePortForwarDirection,
                FirePortOffset = fireAbilityComponent.FirePortOffset,
                FireTime = fireAbilityComponent.FireTime,
                Released = fireAbilityComponent.Released,
                SpecialFireSwitch = true
            });
        }
    }
}
