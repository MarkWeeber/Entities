using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEditor.UIElements;
using UnityEngine;

public class AnimatorBaseAuthoring : MonoBehaviour
{
	public List<AnimatorDotsAsset> aniamtorDotsAssetLists;

	class Baker : Baker<AnimatorBaseAuthoring>
	{
		public override void Bake(AnimatorBaseAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			DynamicBuffer<AnimatorBuffer> animatorBuffer = AddBuffer<AnimatorBuffer>(entity);
            DynamicBuffer<AnimationBuffer> animationBuffer = AddBuffer<AnimationBuffer>(entity);
            DynamicBuffer<AnimatorParametersBuffer> animatorParametersBuffer = AddBuffer<AnimatorParametersBuffer>(entity);
            DynamicBuffer<AnimatorLayerBuffer> animatorLayersBuffer = AddBuffer<AnimatorLayerBuffer>(entity);
            DynamicBuffer<LayerStateBuffer> layerStatesBuffer = AddBuffer<LayerStateBuffer>(entity);
            DynamicBuffer<StateTransitionBuffer> transitionsBuffer = AddBuffer<StateTransitionBuffer>(entity);
            DynamicBuffer<TransitionCondtionBuffer> transitionCondtionsBuffer = AddBuffer<TransitionCondtionBuffer>(entity);
            foreach (var asset in authoring.aniamtorDotsAssetLists)
			{
				var parsedObject = asset.RuntimeAnimatorParsedObject;
				// main animators
                var animatorComponent = new AnimatorBuffer
				{
					Id = asset.AnimatorInstanceId,
					Name = (FixedString32Bytes) asset.AnimatorName,
				};
                animatorBuffer.Add(animatorComponent);
				// animations
                foreach (var item in parsedObject.AnimationBuffer)
				{
                    var animationComponent = new AnimationBuffer
                    {
						Id= item.Id,
						AnimatorInstanceId = item.AnimatorInstanceId,
						Looped = item.Looped,
						Name = item.Name
                    };
                    animationBuffer.Add(animationComponent);
                }
				// animator parameters
				foreach (var item in parsedObject.AnimatorParametersBuffer)
				{
                    var animatorParameterComponent = new AnimatorParametersBuffer
                    {
						Id = item.Id,
						AnimatorInstanceId = item.AnimatorInstanceId,
						DefaultBool = item.DefaultBool,
						DefaultFloat = item.DefaultFloat,
						DefaultInt = item.DefaultInt,
						ParameterName = item.ParameterName,
						Type = item.Type
                    };
					animatorParametersBuffer.Add(animatorParameterComponent);
                }
				// animator layers
				foreach (var item in parsedObject.AnimatorLayerBuffer)
				{
                    var animatorLayerComponent = new AnimatorLayerBuffer
                    {
						Id = item.Id,
						AnimatorInstanceId = item.AnimatorInstanceId,
						DefaultWeight = item.DefaultWeight,
                    };
                    animatorLayersBuffer.Add(animatorLayerComponent);
                }
				// states in layers
				foreach (var item in parsedObject.LayerStateBuffer)
				{
                    var layerStateComponent = new LayerStateBuffer
                    {
						Id = item.Id,
						AnimatorInstanceId = item.AnimatorInstanceId,
						AnimationClipId = item.AnimationClipId,
						DefaultState = item.DefaultState,
						LayerId = item.LayerId,
						Speed = item.Speed,
						StateName = item.StateName
                    };
					layerStatesBuffer.Add(layerStateComponent);
                }
				// state transitions
				foreach (var item in parsedObject.StateTransitionBuffer)
				{
                    var stateTransitionComponent = new StateTransitionBuffer
                    {
						Id = item.Id,
						AnimatorInstanceId = item.AnimatorInstanceId,
						DestinationStateId = item.DestinationStateId,
						DestinationStateName = item.DestinationStateName,
						ExitTime = item.ExitTime,
						HasExitTime = item.HasExitTime,
						StateId	= item.StateId,
						TransitionDuration = item.TransitionDuration,
						TransitionOffset = item.TransitionOffset
                    };
                    transitionsBuffer.Add(stateTransitionComponent);
                }
				// transition conditions
				foreach (var item in parsedObject.TransitionCondtionBuffer)
				{
                    var transitionCondtionComponent = new TransitionCondtionBuffer
                    {
						Id = item.Id,
						AnimatorInstanceId= item.AnimatorInstanceId,
						Mode = item.Mode,
						Parameter = item.Parameter,
						TransitionId = item.TransitionId,
						Treshold = item.Treshold
                    };
                    transitionCondtionsBuffer.Add(transitionCondtionComponent);
                }
            }
		}
	}
}