using Unity.Entities;
using UnityEngine;

public class ProjectileSpawnerDataAuthoring : MonoBehaviour
{
	public GameObject ProjectilePrefab;
	public GameObject SpecialProjectilePrefab;

	class Baker : Baker<ProjectileSpawnerDataAuthoring>
	{
		public override void Bake(ProjectileSpawnerDataAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, new ProjectileSpawnerData
			{
				Projectile = GetEntity(authoring.ProjectilePrefab, TransformUsageFlags.Dynamic),
				SpecialProjectile = GetEntity(authoring.SpecialProjectilePrefab, TransformUsageFlags.Dynamic)
			});
		}
	}
}