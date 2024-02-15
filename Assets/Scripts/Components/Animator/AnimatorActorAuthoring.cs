using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class AnimatorActorAuthoring : MonoBehaviour
{
    public AnimatorDotsAsset animatorDotsAsset;
    class Baker : Baker<AnimatorActorAuthoring>
    {
        public override void Bake(AnimatorActorAuthoring authoring)
        {
            if (authoring.animatorDotsAsset == null)
            {
                return;
            }
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new AnimatorActorComponent
            {
                AnimatorId = authoring.animatorDotsAsset.AnimatorInstanceId,
            });
            RegisterBuffers(entity, authoring.animatorDotsAsset);
            DynamicBuffer<AnimatorActorPartBufferComponent> animatorActorPartComponents = AddBuffer<AnimatorActorPartBufferComponent>(entity);
            RegisterChildren(authoring.gameObject, ref animatorActorPartComponents, "");
        }

        private void RegisterBuffers(Entity entity, AnimatorDotsAsset asset)
        {
            DynamicBuffer<AnimationBuffer> animationBuffer = AddBuffer<AnimationBuffer>(entity);
            DynamicBuffer<AnimatorActorParametersBuffer> animatorActorParametersBuffer = AddBuffer<AnimatorActorParametersBuffer>(entity);
            DynamicBuffer<AnimatorActorLayerBuffer> animatorActorLayersBuffer = AddBuffer<AnimatorActorLayerBuffer>(entity);
            DynamicBuffer<LayerStateBuffer> layerStatesBuffer = AddBuffer<LayerStateBuffer>(entity);
            DynamicBuffer<StateTransitionBuffer> transitionsBuffer = AddBuffer<StateTransitionBuffer>(entity);
            DynamicBuffer<TransitionCondtionBuffer> transitionCondtionsBuffer = AddBuffer<TransitionCondtionBuffer>(entity);
            DynamicBuffer<AnimationPositionBuffer> animationPositionsBuffer = AddBuffer<AnimationPositionBuffer>(entity);
            DynamicBuffer<AnimationRotationBuffer> animationRotationsBuffer = AddBuffer<AnimationRotationBuffer>(entity);

            var parsedObject = asset.RuntimeAnimatorParsedObject;
            // animations
            foreach (var item in parsedObject.Animations)
            {
                var animationComponent = new AnimationBuffer
                {
                    Id = item.Id,
                    AnimatorInstanceId = item.AnimatorInstanceId,
                    Looped = item.Looped,
                    Length = item.Length,
                };
                animationBuffer.Add(animationComponent);
            }
            // animation position keys
            foreach (var positionKey in parsedObject.Positions)
            {
                var positionKeyComponent = new AnimationPositionBuffer
                {
                    AnimationId = positionKey.AnimationId,
                    Path = (FixedString512Bytes)positionKey.Path,
                    Time = positionKey.Time,
                    Value = positionKey.Value,
                };
                animationPositionsBuffer.Add(positionKeyComponent);

            }
            // animation rotation keys
            foreach (var rotationKey in parsedObject.Rotations)
            {
                var rotationKeyComponent = new AnimationRotationBuffer
                {
                    AnimationId = rotationKey.AnimationId,
                    Path = (FixedString512Bytes)rotationKey.Path,
                    Time = rotationKey.Time,
                    Value = rotationKey.Value,
                };
                animationRotationsBuffer.Add(rotationKeyComponent);
            }
            // animator parameters
            foreach (var parameter in parsedObject.AnimatorParameters)
            {
                float defaultNumericValue = 0;
                switch (parameter.Type)
                {
                    case UnityEngine.AnimatorControllerParameterType.Float:
                        defaultNumericValue = parameter.DefaultFloat;
                        break;
                    case UnityEngine.AnimatorControllerParameterType.Int:
                        defaultNumericValue = parameter.DefaultInt;
                        break;
                    default:
                        break;
                }
                var actorParameterItem = new AnimatorActorParametersBuffer
                {
                    ParameterName = parameter.ParameterName,
                    Type = parameter.Type,
                    NumericValue = defaultNumericValue,
                    BoolValue = parameter.DefaultBool
                };
                animatorActorParametersBuffer.Add(actorParameterItem);
            }

            // state transitions
            foreach (var item in parsedObject.StateTransitions)
            {
                var stateTransitionComponent = new StateTransitionBuffer
                {
                    Id = item.Id,
                    AnimatorInstanceId = item.AnimatorInstanceId,
                    DestinationStateId = item.DestinationStateId,
                    ExitTime = item.ExitTime,
                    FixedDuration = item.FixedDuration,
                    StateId = item.StateId,
                    TransitionDuration = item.TransitionDuration,
                    TransitionOffset = item.TransitionOffset
                };
                transitionsBuffer.Add(stateTransitionComponent);
            }

            // transition conditions
            foreach (var item in parsedObject.TransitionCondtions)
            {
                var transitionCondtionComponent = new TransitionCondtionBuffer
                {
                    Id = item.Id,
                    AnimatorInstanceId = item.AnimatorInstanceId,
                    Mode = item.Mode,
                    Parameter = item.Parameter,
                    TransitionId = item.TransitionId,
                    Treshold = item.Treshold
                };
                transitionCondtionsBuffer.Add(transitionCondtionComponent);
            }

            // layers and states
            foreach (var layer in parsedObject.AnimatorLayers)
            {
                int defaultStateId = -1;
                var layerDefaultAnimationClip = new AnimationBuffer();
                var defaultState = new LayerStateBuffer();
                // states
                foreach (var state in parsedObject.LayerStates)
                {
                    if (layer.Id == state.LayerId && state.DefaultState)
                    {
                        defaultStateId = state.Id;
                        defaultState = state;
                    }
                    var animationClipForState = new AnimationBuffer();
                    foreach (var animation in parsedObject.Animations) // find animation clip for this state
                    {
                        if (state.AnimationClipId == animation.Id)
                        {
                            animationClipForState = animation;
                            break;
                        }
                    }
                    var layerStateComponent = new LayerStateBuffer
                    {
                        Id = state.Id,
                        AnimatorInstanceId = state.AnimatorInstanceId,
                        AnimationClipId = state.AnimationClipId,
                        DefaultState = state.DefaultState,
                        LayerId = state.LayerId,
                        Speed = state.Speed,
                        AnimationLength = animationClipForState.Length,
                        AnimationLooped = animationClipForState.Looped,
                    };
                    layerStatesBuffer.Add(layerStateComponent);
                }
                foreach (var animation in parsedObject.Animations)
                {
                    if (defaultState.AnimationClipId == animation.Id)
                    {
                        layerDefaultAnimationClip = animation;
                        break;
                    }
                }
                var actorLayerItem = new AnimatorActorLayerBuffer
                {
                    Id = layer.Id,
                    DefaultWeight = layer.DefaultWeight,

                    // current state and animation info
                    CurrentStateId = defaultStateId,
                    CurrentStateSpeed = defaultState.Speed,
                    CurrentAnimationId = layerDefaultAnimationClip.Id,
                    CurrentAnimationTime = 0f, // time needed for animation
                    CurrentAnimationLength = layerDefaultAnimationClip.Length,
                    CurrentAnimationIsLooped = layerDefaultAnimationClip.Looped,

                    // transition info
                    IsInTransition = false, // main transition switch
                    TransitionDuration = 0f, // actual transition duration
                    TransitionTimer = 0f, // actual transition timer

                    FirstOffsetTimer = 0f, // start offset timer
                    SecondAnimationOffset = 0f, // offset for second animation start

                    // second state and animation info
                    NextStateId = 0,
                    NextStateSpeed = 1f,
                    NextAnimationId = 0,
                    NextAnimationTime = 0f, // time needed in transitioning animation
                    NextAnimationLength = 0f,
                    NextAnimationSpeed = 1f,
                    NextAnimationIsLooped = false,
                };
                animatorActorLayersBuffer.Add(actorLayerItem);
            }
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