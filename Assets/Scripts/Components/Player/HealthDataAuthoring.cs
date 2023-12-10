using Unity.Entities;
using UnityEngine;

public class HealthDataAuthoring : MonoBehaviour
{
	public float CurrentHealth = 100f;
	public float MaxHealth = 100f;
	class Baker : Baker<HealthDataAuthoring>
	{
		public override void Bake(HealthDataAuthoring authoring)
		{
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new HealthData
            {
                CurrentHealth = authoring.CurrentHealth,
				MaxHealth = authoring.MaxHealth
            });
        }
	}
}