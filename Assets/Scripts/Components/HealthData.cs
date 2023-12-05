using Unity.Entities;

public struct HealthData : IComponentData, IEnableableComponent
{
    public float CurrentHealth;
    public float MaxHealth;
}