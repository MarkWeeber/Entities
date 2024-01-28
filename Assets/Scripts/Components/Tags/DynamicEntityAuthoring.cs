using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class DynamicEntityAuthoring : MonoBehaviour
{

	class Baker : Baker<DynamicEntityAuthoring>
	{
		public override void Bake(DynamicEntityAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new DynamicEntityTag());
            AddComponent<LocalTransform>(entity);
        }
	}
}