using ParseUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class AnimationBaseAuthoring : MonoBehaviour
{
	public List<AnimationDotsAsset> Animations;
	class Baker : Baker<AnimationBaseAuthoring>
	{
		public override void Bake(AnimationBaseAuthoring authoring)
		{
			if (authoring.Animations == null)
			{
				return;
			}
            Entity entity = GetEntity(TransformUsageFlags.None);
			RegisterAnimations(entity, authoring);
            RegisterRotations(entity, authoring);
            RegisterPositions(entity, authoring);
        }

		private void RegisterAnimations(Entity entity, AnimationBaseAuthoring authoring)
		{
			var animations = AddBuffer<AnimationBuffer>(entity);
			foreach (var asset in authoring.Animations)
			{
				animations.Add(new AnimationBuffer
				{
					AnimationInstanceId = asset.AnimationClipParsedObject.Id,
					AnimatorInstanceId = asset.AnimationClipParsedObject.AnimatorInstanceId,
					Length = asset.AnimationClipParsedObject.Length,
					Looped = asset.AnimationClipParsedObject.Looped,
					Name = (FixedString32Bytes) asset.AnimationClipParsedObject.AnimationName,
				});
            }
		}

		private void RegisterRotations(Entity entity, AnimationBaseAuthoring authoring)
		{
            var rotations = AddBuffer<AnimationRotationBuffer>(entity);
            foreach (var asset in authoring.Animations)
            {
				foreach (var item in asset.AnimationClipParsedObject.Rotations)
				{
					rotations.Add(new AnimationRotationBuffer
					{
						AnimationId = item.AnimationId,
						Path = (FixedString512Bytes)item.Path,
						Time = item.Time,
						Value = item.Value
					});
				}
            }
        }

		private void RegisterPositions(Entity entity, AnimationBaseAuthoring authoring)
		{
            var positions = AddBuffer<AnimationPositionBuffer>(entity);
            foreach (var asset in authoring.Animations)
            {
                foreach (var item in asset.AnimationClipParsedObject.Positions)
                {
                    positions.Add(new AnimationPositionBuffer
                    {
                        AnimationId = item.AnimationId,
                        Path = (FixedString512Bytes)item.Path,
                        Time = item.Time,
                        Value = item.Value
                    });
                }
            }
        }
	}
}