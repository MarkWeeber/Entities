using Unity.Entities;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatorControllerAuthoring : MonoBehaviour
{
	public AnimatorController animatorController;
	class Baker : Baker<AnimatorControllerAuthoring>
	{
		public override void Bake(AnimatorControllerAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.None);
			AddComponentObject(entity, new AnimatorControllerComponent { Value = authoring.animatorController });
        }
	}
}