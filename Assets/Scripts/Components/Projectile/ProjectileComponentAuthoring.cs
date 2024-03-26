using Unity.Entities;
using UnityEngine;

public class ProjectileComponentAuthoring : MonoBehaviour
{
	[SerializeField]
	private float Damage = 12f;
	class Baker : Baker<ProjectileComponentAuthoring>
	{
		public override void Bake(ProjectileComponentAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new ProjectileComponent
			{
				Damage = authoring.Damage
			});
		}
	}
}