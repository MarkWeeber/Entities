using Unity.Entities;
using UnityEngine;

public class HealthReplenishItem : MonoBehaviour, IItem, IConsumable
{
    [SerializeField] private float healthReplenishAmount;
    public void PickUp()
    {
        Debug.Log("Health Item picked for " + healthReplenishAmount + " points");
    }

    public void Drop()
    {

    }

    public void Consume(Entity entity, EntityManager entityManager)
    {
        if(entityManager.HasComponent<HealthData>(entity))
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
