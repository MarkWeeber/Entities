using Unity.Collections;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

public class AnimatorActorAuthoring : MonoBehaviour
{
	public string AnimatorName;
	class Baker : Baker<AnimatorActorAuthoring>
	{
		public override void Bake(AnimatorActorAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new AnimatorActorComponent
			{
				AnimatorControllerName = (FixedString32Bytes)authoring.AnimatorName,
				AnimatorControllerEntity = Entity.Null
			});
		}
	}
}