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
				AnimatorId = -1,
				AnimatorControllerName = (FixedString32Bytes)authoring.AnimatorName
			});
			DynamicBuffer<AnimatorActorPartBufferComponent> animatorActorPartComponents = AddBuffer<AnimatorActorPartBufferComponent>(entity);
			//string rootPathName = authoring.gameObject.name;
			// animatorActorPartComponents.Add(new AnimatorActorPartBufferComponent
			// {
			// 	Value = entity,
			// 	Path = (FixedString512Bytes)rootPathName
			// });
			RegisterChildren(authoring.gameObject, ref animatorActorPartComponents, "");
		}

		private void RegisterChildren(GameObject gameObject, ref DynamicBuffer<AnimatorActorPartBufferComponent> animatorActorParts, string pathName)
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
				string currentPathName = pathName + go.name;
				Entity entity = GetEntity(go, TransformUsageFlags.Dynamic);
				animatorActorParts.Add(new AnimatorActorPartBufferComponent
				{
					Path = (FixedString512Bytes)currentPathName,
					Value = entity
				});
				RegisterChildren(go, ref animatorActorParts, currentPathName + "/");
			}
        }
	}
}