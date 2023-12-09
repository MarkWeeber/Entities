using Unity.Entities;
using UnityEngine;

public class RayCasterTagAuthoring : MonoBehaviour
{

	class Baker : Baker<RayCasterTagAuthoring>
	{
		public override void Bake(RayCasterTagAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new RayCasterTag { });
		}
	}
}