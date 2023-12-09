using Unity.Entities;
using UnityEngine;

public class DestructibleProjectileTagAuthoring : MonoBehaviour
{

	class Baker : Baker<DestructibleProjectileTagAuthoring>
	{
		public override void Bake(DestructibleProjectileTagAuthoring authoring)
		{
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DestructibleProjectileTag { });
        }
	}
}