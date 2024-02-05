using System.Collections.Generic;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatorBaseControllerAuthoring : MonoBehaviour
{
	public List <AnimatorController> animatorControllers;
	public GameObject EmptyGameObject;
	class Baker : Baker<AnimatorBaseControllerAuthoring>
	{
		public override void Bake(AnimatorBaseControllerAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			Entity emtpyEntity = GetEntity(authoring.EmptyGameObject, TransformUsageFlags.Dynamic);
			AddComponentObject(entity, new AnimatorBaseControllerComponent
			{
				Value = authoring.animatorControllers,
				EmptyEntity = emtpyEntity
            });
        }
	}
}