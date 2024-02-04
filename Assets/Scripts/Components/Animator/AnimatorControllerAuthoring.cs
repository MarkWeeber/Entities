using System.Collections.Generic;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatorControllerAuthoring : MonoBehaviour
{
	public List <AnimatorController> animatorControllers;
	public GameObject EmptyGameObject;
	class Baker : Baker<AnimatorControllerAuthoring>
	{
		public override void Bake(AnimatorControllerAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			Entity emtpyEntity = GetEntity(authoring.EmptyGameObject, TransformUsageFlags.Dynamic);
			AddComponentObject(entity, new AnimatorControllerComponent
			{
				Value = authoring.animatorControllers,
				EmptyEntity = emtpyEntity
            });
        }
	}
}