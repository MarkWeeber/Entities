using Unity.Entities;
using UnityEngine;

public class ItemHealAction : IItemAction
{
    private float healthReplenishAmount;
    public float HealthReplenishAmount { get => healthReplenishAmount; set => healthReplenishAmount = value; }
    private string actionName;
    public string ActionName { get => actionName; set => actionName = value; }
    private Entity entity;
    public Entity Entity { get => entity; set => entity = value; }
    private EntityManager entityManager;
    public EntityManager EntityManager { get => entityManager; set => entityManager = value; }

    public ItemHealAction(string actionName, float healthReplenishAmount)
    {
        this.actionName = actionName;
        this.healthReplenishAmount = healthReplenishAmount;
    }

    public void ActivateAction()
    {
        if (entityManager.HasComponent<HealthData>(entity))
        {
            var healthData = entityManager.GetComponentData<HealthData>(entity);
            float healthToBeReplenished = Mathf.Min(healthReplenishAmount, healthData.MaxHealth - healthData.CurrentHealth);
            entityManager.SetComponentData(entity, new HealthData
            {
                MaxHealth = healthData.MaxHealth,
                CurrentHealth = healthData.CurrentHealth + healthToBeReplenished
            });
        }
    }
}
