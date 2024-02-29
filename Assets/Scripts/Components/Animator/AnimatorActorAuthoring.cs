using ParseUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class AnimatorActorAuthoring : MonoBehaviour
{
    public AnimatorDotsAsset animatorDotsAsset;
    public PartsAnimationMethod Method;
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
            RegisterBuffers(entity, authoring.animatorDotsAsset, authoring.Method);
            DynamicBuffer<AnimatorActorPartBufferComponent> animatorActorPartComponents = AddBuffer<AnimatorActorPartBufferComponent>(entity);
            RegisterChildren(entity, authoring.gameObject, ref animatorActorPartComponents, "");
        }

        private void RegisterBuffers(Entity entity, AnimatorDotsAsset asset, PartsAnimationMethod method)
        {
            DynamicBuffer<AnimatorActorParametersBuffer> animatorActorParametersBuffer = AddBuffer<AnimatorActorParametersBuffer>(entity);
            DynamicBuffer<AnimatorActorLayerBuffer> animatorActorLayersBuffer = AddBuffer<AnimatorActorLayerBuffer>(entity);

            var parsedObject = asset.RuntimeAnimatorParsedObject;

            // animator parameters
            foreach (var parameter in parsedObject.AnimatorParameters)
            {
                float defaultNumericValue = 0;
                switch (parameter.Type)
                {
                    case AnimatorControllerParameterType.Float:
                        defaultNumericValue = parameter.DefaultFloat;
                        break;
                    case AnimatorControllerParameterType.Int:
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

            // layers and states
            foreach (var layer in parsedObject.AnimatorLayers)
            {
                int defaultStateId = -1;
                var layerDefaultAnimationClip = new AnimationItem();
                var defaultState = new LayerStateBuffer();
                // states
                foreach (var state in parsedObject.LayerStates)
                {
                    if (layer.Id == state.LayerId && state.DefaultState)
                    {
                        defaultStateId = state.Id;
                        defaultState = state;
                        break;
                    }
                }
                foreach (var animation in parsedObject.Animations)
                {
                    if (defaultState.AnimationClipId == animation.AnimationInstanceId)
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
                    CurrentAnimationId = layerDefaultAnimationClip.AnimationInstanceId,
                    CurrentAnimationBlobIndex = defaultState.AnimationBlobAssetIndex,
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
                    Method = method
                };
                animatorActorLayersBuffer.Add(actorLayerItem);
            }
        }
        private void RegisterChildren(Entity rootEntity, GameObject gameObject, ref DynamicBuffer<AnimatorActorPartBufferComponent> animatorActorParts, string pathName)
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
                    Value = entity,
                    RootEntity = rootEntity,
                    SetNewLocalTransform = false
                });
                RegisterChildren(rootEntity, go, ref animatorActorParts, currentPathName + "/");
            }
        }
    }
}