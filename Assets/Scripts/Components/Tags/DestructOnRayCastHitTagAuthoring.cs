using Unity.Entities;
using UnityEngine;

public class DestructOnRayCastHitTagAuthoring : MonoBehaviour
{

	class Baker : Baker<DestructOnRayCastHitTagAuthoring>
	{
		public override void Bake(DestructOnRayCastHitTagAuthoring authoring)
		{
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DestructOnRayCastHitTag { });
        }
	}
}