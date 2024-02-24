using ParseUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class AnimatorBaseAuthoring : MonoBehaviour
{
	public List<AnimatorDotsAsset> Animators;
	class Baker : Baker<AnimatorBaseAuthoring>
	{
		public override void Bake(AnimatorBaseAuthoring authoring)
		{
			if (authoring.Animators == null)
			{
				return;
			}
			Entity entity = GetEntity(TransformUsageFlags.None);
			RegisterAnimators(entity, authoring);
            RegisterParameters(entity, authoring);
            RegisterLayers(entity, authoring);
            RegisterTransitions(entity, authoring);
            RegisterConditions(entity, authoring);
        }

		private void RegisterAnimators(Entity entity, AnimatorBaseAuthoring authoring)
		{
            var animators = AddBuffer<AnimatorBuffer>(entity);
			foreach (var animatorDotsAsset in authoring.Animators)
			{
				if (animatorDotsAsset == null)
				{
					continue;
				}
				animators.Add(new AnimatorBuffer
				{
					Id = animatorDotsAsset.AnimatorInstanceId,
					Name = (FixedString32Bytes) animatorDotsAsset.AnimatorName
				});
            }
        }

		private void RegisterParameters(Entity entity, AnimatorBaseAuthoring authoring)
		{
			var parameters = AddBuffer<AnimatorParameterBuffer>(entity);
			foreach (var animatorDotsAsset in authoring.Animators)
			{
                if (animatorDotsAsset == null)
                {
                    continue;
                }
                foreach (var item in animatorDotsAsset.RuntimeAnimatorParsedObject.AnimatorParameters)
				{
					parameters.Add(new AnimatorParameterBuffer
					{
						AnimatorInstanceId = item.AnimatorInstanceId,
						DefaultBool = item.DefaultBool,
						DefaultFloat = item.DefaultFloat,
						DefaultInt = item.DefaultInt,
						Id = item.Id,
						ParameterName = (FixedString32Bytes)item.ParameterName,
						Type = item.Type
					});
                }
			}
		}

		private void RegisterLayers(Entity entity, AnimatorBaseAuthoring authoring)
		{
            var layers = AddBuffer<LayerStateBuffer>(entity);
            foreach (var animatorDotsAsset in authoring.Animators)
            {
                if (animatorDotsAsset == null)
                {
                    continue;
                }
                foreach (var item in animatorDotsAsset.RuntimeAnimatorParsedObject.LayerStates)
                {
                    layers.Add(new LayerStateBuffer
					{
						Id = item.Id,
						AnimationClipId = item.AnimationClipId,
						AnimationLength = item.AnimationLength,
						AnimationLooped = item.AnimationLooped,
						AnimatorInstanceId = item.AnimatorInstanceId,
						DefaultState = item.DefaultState,
						LayerId = item.LayerId,
						Speed = item.Speed
					});
                }
            }
        }

		private void RegisterTransitions(Entity entity, AnimatorBaseAuthoring authoring)
		{
            var transitions = AddBuffer<StateTransitionBuffer>(entity);
            foreach (var animatorDotsAsset in authoring.Animators)
            {
                if (animatorDotsAsset == null)
                {
                    continue;
                }
                foreach (var item in animatorDotsAsset.RuntimeAnimatorParsedObject.StateTransitions)
                {
                    transitions.Add(new StateTransitionBuffer
					{
						AnimatorInstanceId = item.AnimatorInstanceId,
						DestinationStateId = item.DestinationStateId,
						ExitTime = item.ExitTime,
						FixedDuration = item.FixedDuration,
						Id = item.Id,
						StateId = item.StateId,
						TransitionDuration = item.TransitionDuration,
						TransitionOffset = item.TransitionOffset
					});
                }
            }
        }
		private void RegisterConditions(Entity entity, AnimatorBaseAuthoring authoring)
		{
            var conditions = AddBuffer<TransitionCondtionBuffer>(entity);
            foreach (var animatorDotsAsset in authoring.Animators)
            {
                if (animatorDotsAsset == null)
                {
                    continue;
                }
                foreach (var item in animatorDotsAsset.RuntimeAnimatorParsedObject.TransitionCondtions)
                {
                    conditions.Add(new TransitionCondtionBuffer
					{
						Id = item.Id,
						AnimatorInstanceId = item.AnimatorInstanceId,
						Mode = item.Mode,
						Parameter = (FixedString32Bytes)item.Parameter,
						TransitionId = item.TransitionId,
						Treshold = item.Treshold
					});
                }
            }
        }
	}
}