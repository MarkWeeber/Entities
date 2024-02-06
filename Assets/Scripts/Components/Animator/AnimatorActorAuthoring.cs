using System.Collections.Generic;
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
				AnimatorControllerName = (FixedString32Bytes)authoring.AnimatorName
			});
			AddComponent(entity, new AnimatorActorBuilderComponent { });
			DynamicBuffer<AnimatorActorPartComponent> animatorActorPartComponents = AddBuffer<AnimatorActorPartComponent>(entity);
			string rootPathName = authoring.gameObject.name;
			animatorActorPartComponents.Add(new AnimatorActorPartComponent
			{
				Value = entity,
				Path = (FixedString512Bytes)rootPathName
			});
			RegisterChildren(authoring.gameObject, ref animatorActorPartComponents, rootPathName);
		}

		private void RegisterChildren(GameObject gameObject, ref DynamicBuffer<AnimatorActorPartComponent> animatorActorParts, string pathName)
        {
			List<GameObject> children = new List<GameObject>();
			GetChildren(gameObject, children, false);
			string localPathName = pathName;
            foreach (GameObject go in children)
            {
                if (go == gameObject)
                {
					continue;
                }
				string currentPathName = pathName + "/" + go.name;
				Entity entity = GetEntity(go, TransformUsageFlags.Dynamic);
				animatorActorParts.Add(new AnimatorActorPartComponent
				{
					Path = (FixedString512Bytes)currentPathName,
					Value = entity
				});
				RegisterChildren(go, ref animatorActorParts, currentPathName);
			}
        }
	}
}