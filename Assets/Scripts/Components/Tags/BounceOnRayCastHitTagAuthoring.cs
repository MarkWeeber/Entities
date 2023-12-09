using Unity.Entities;
using UnityEngine;

public class BounceOnRayCastHitTagAuthoring : MonoBehaviour
{

	class Baker : Baker<BounceOnRayCastHitTagAuthoring>
	{
		public override void Bake(BounceOnRayCastHitTagAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new BounceOnRayCastHitTag { });
		}
	}
}