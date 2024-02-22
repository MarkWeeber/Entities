using Unity.Entities;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
	public int SpawnQuantity = 0;
	public float Spacing = 1f;
	public GameObject Prefab;
	class Baker : Baker<SpawnerAuthoring>
	{
		public override void Bake(SpawnerAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, new SpawnerComponent
			{
				Quantity = authoring.SpawnQuantity,
				Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
				SpawnOriginPosition = authoring.transform.position,
				Spacing = authoring.Spacing
			});
		}
	}
}