using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SetLocalTransformAuthoring : MonoBehaviour
{

	class Baker : Baker<SetLocalTransformAuthoring>
	{
		public override void Bake(SetLocalTransformAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new SetLocalTransformComponent
			{
				Set = false,
				SetPosition = float3.zero,
				SetRotation = Quaternion.identity,
				SetScale = 1f
			});
		}
	}
}