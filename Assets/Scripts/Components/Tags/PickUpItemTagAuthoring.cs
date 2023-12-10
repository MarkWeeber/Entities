using Unity.Entities;
using UnityEngine;

public class PickUpItemTagAuthoring : MonoBehaviour
{

	class Baker : Baker<PickUpItemTagAuthoring>
	{
		public override void Bake(PickUpItemTagAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new PickUpItemTag { });
		}
	}
}