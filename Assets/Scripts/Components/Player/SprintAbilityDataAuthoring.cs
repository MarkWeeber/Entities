using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SprintAbilityDataAuthoring : MonoBehaviour
{
	public float SpeedMultiplier = 1.7f;
	public float SprintTime = 1f;

	class Baker : Baker<SprintAbilityDataAuthoring>
	{
		public override void Bake(SprintAbilityDataAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new SprintAbilityData()
			{
				SpeedMultiplier = authoring.SpeedMultiplier,
				SprintTime = authoring.SprintTime,
				Active = false,
				Released = false
			});
		}
	}
}