using Unity.Entities;
using UnityEngine;

public class BoneTagAuthoring : MonoBehaviour
{

	class Baker : Baker<BoneTagAuthoring>
	{
		public override void Bake(BoneTagAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new BoneTag { });
		}
	}
}