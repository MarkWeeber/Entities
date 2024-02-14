//using System.Collections.Generic;
//using System.Linq;
//using Unity.Collections;
//using Unity.Entities;
//using UnityEditor.UIElements;
//using UnityEngine;

//public class AnimatorBaseAuthoringObsolete : MonoBehaviour
//{
//	public List<AnimatorDotsAsset> aniamtorDotsAssetLists;

//	class Baker : Baker<AnimatorBaseAuthoringObsolete>
//	{
//		public override void Bake(AnimatorBaseAuthoringObsolete authoring)
//		{
//			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
//			DynamicBuffer<AnimatorBuffer> animatorBuffer = AddBuffer<AnimatorBuffer>(entity);
//            DynamicBuffer<AnimationBuffer> animationBuffer = AddBuffer<AnimationBuffer>(entity);
//            DynamicBuffer<LayerStateBuffer> layerStatesBuffer = AddBuffer<LayerStateBuffer>(entity);
//            DynamicBuffer<StateTransitionBuffer> transitionsBuffer = AddBuffer<StateTransitionBuffer>(entity);
//            DynamicBuffer<TransitionCondtionBuffer> transitionCondtionsBuffer = AddBuffer<TransitionCondtionBuffer>(entity);
//			DynamicBuffer<AnimationPositionBuffer> animationPositionsBuffer = AddBuffer<AnimationPositionBuffer>(entity);
//			DynamicBuffer<AnimationRotationBuffer> animationRotationsBuffer = AddBuffer<AnimationRotationBuffer>(entity);
//            foreach (var asset in authoring.aniamtorDotsAssetLists)
//			{
//				if (asset == null)
//				{
//					continue;
//				}
//				var parsedObject = asset.RuntimeAnimatorParsedObject;
//				// main animators
//                var animatorComponent = new AnimatorBuffer
//				{
//					Id = asset.AnimatorInstanceId,
//					Name = (FixedString32Bytes) asset.AnimatorName,
//				};
//                animatorBuffer.Add(animatorComponent);
//				// animations
//                foreach (var item in parsedObject.Animations)
//				{
//                    var animationComponent = new AnimationBuffer
//                    {
//						Id = item.Id,
//						AnimatorInstanceId = item.AnimatorInstanceId,
//						Looped = item.Looped,
//						Length = item.Length,
//                    };
//                    animationBuffer.Add(animationComponent);
//                }
//				// animation position keys
//				foreach (var positionKey in parsedObject.Positions)
//				{
//					var positionKeyComponent = new AnimationPositionBuffer
//					{
//						AnimationId = positionKey.AnimationId,
//						Path = (FixedString512Bytes) positionKey.Path,
//						Time = positionKey.Time,
//						Value = positionKey.Value,
//					};
//					animationPositionsBuffer.Add(positionKeyComponent);

//                }
//				// animation rotation keys
//				foreach (var rotationKey in parsedObject.Rotations)
//				{
//					var rotationKeyComponent = new AnimationRotationBuffer
//					{
//						AnimationId = rotationKey.AnimationId,
//						Path = (FixedString512Bytes) rotationKey.Path,
//						Time = rotationKey.Time,
//						Value = rotationKey.Value,
//					};
//                    animationRotationsBuffer.Add(rotationKeyComponent);
//                }
//				// states in layers
//				foreach (var item in parsedObject.LayerStates)
//				{
//                    var layerStateComponent = new LayerStateBuffer
//                    {
//						Id = item.Id,
//						AnimatorInstanceId = item.AnimatorInstanceId,
//						AnimationClipId = item.AnimationClipId,
//						DefaultState = item.DefaultState,
//						LayerId = item.LayerId,
//						Speed = item.Speed
//                    };
//					layerStatesBuffer.Add(layerStateComponent);
//                }
//				// state transitions
//				foreach (var item in parsedObject.StateTransitions)
//				{
//                    var stateTransitionComponent = new StateTransitionBuffer
//                    {
//						Id = item.Id,
//						AnimatorInstanceId = item.AnimatorInstanceId,
//						DestinationStateId = item.DestinationStateId,
//						ExitTime = item.ExitTime,
//						HasExitTime = item.HasExitTime,
//						StateId	= item.StateId,
//						TransitionDuration = item.TransitionDuration,
//						TransitionOffset = item.TransitionOffset
//                    };
//                    transitionsBuffer.Add(stateTransitionComponent);
//                }
//				// transition conditions
//				foreach (var item in parsedObject.TransitionCondtions)
//				{
//                    var transitionCondtionComponent = new TransitionCondtionBuffer
//                    {
//						Id = item.Id,
//						AnimatorInstanceId= item.AnimatorInstanceId,
//						Mode = item.Mode,
//						Parameter = item.Parameter,
//						TransitionId = item.TransitionId,
//						Treshold = item.Treshold
//                    };
//                    transitionCondtionsBuffer.Add(transitionCondtionComponent);
//                }
//            }
//		}
//	}
//}