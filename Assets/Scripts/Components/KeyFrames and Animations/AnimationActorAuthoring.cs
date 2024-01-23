using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class AnimationActorAuthoring : MonoBehaviour
{
    public string namePlaceHolder;
    class Baker : Baker<AnimationActorAuthoring>
	{
		public override void Bake(AnimationActorAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new AnimationActorComponent
			{
				AnimationName = (FixedString32Bytes)authoring.namePlaceHolder,
				AnimationTime = 0f,
			});
			DynamicBuffer<AnimationPartComponent> animationPartComponents = AddBuffer<AnimationPartComponent>(entity);
			foreach (GameObject go in GetChildren(includeChildrenRecursively: true))
			{
				if (go == authoring.gameObject)
				{
					continue;
				}
				Entity _entity = GetEntity(go, TransformUsageFlags.Dynamic);
				animationPartComponents.Add(new AnimationPartComponent
				{
					Name = (FixedString32Bytes)go.name,
					Entity = _entity,
				});
			}
		}
	}
}