using Unity.Entities;
using UnityEngine;

public class ProjectileTagAuthoring : MonoBehaviour
{

	class Baker : Baker<ProjectileTagAuthoring>
	{
		public override void Bake(ProjectileTagAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new ProjectileTag { });
		}
	}
}