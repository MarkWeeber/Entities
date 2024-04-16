using Unity.Entities;
using UnityEngine;

public class DeformationsSineSpeedOverrideAuthoring : MonoBehaviour
{
	[SerializeField] private float sineSpeed = 5f;
	class Baker : Baker<DeformationsSineSpeedOverrideAuthoring>
	{
		public override void Bake(DeformationsSineSpeedOverrideAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Renderable);
			AddComponent(entity, new DeformationsSineSpeedOverride
			{
				Value = authoring.sineSpeed
			});
		}
	}
}