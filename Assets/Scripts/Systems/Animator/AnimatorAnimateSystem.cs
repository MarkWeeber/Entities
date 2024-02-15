using System;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct AnimatorAnimateSystem : ISystem
{
    private ComponentLookup<AnimatorActorPartComponent> partComponentLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimatorActorComponent>();
        state.RequireForUpdate<AnimatorActorPartComponent>();
        partComponentLookup = state.GetComponentLookup<AnimatorActorPartComponent>(false);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityQuery acrtorsQuery = SystemAPI.QueryBuilder()
            .WithAll<
             AnimationBuffer,
             AnimatorActorParametersBuffer,
             AnimatorActorPartBufferComponent,
             AnimatorActorLayerBuffer,
             LayerStateBuffer,
             StateTransitionBuffer,
             TransitionCondtionBuffer>()
            .Build();

        if (acrtorsQuery.CalculateEntityCount() < 1)
        {
            return;
        }

        float deltaTime = SystemAPI.Time.DeltaTime;
        partComponentLookup.Update(ref state);

        state.Dependency = new ActorAnimateJob
        {
            PartComponentLookup = partComponentLookup,
            DeltaTime = deltaTime
        }.ScheduleParallel(acrtorsQuery, state.Dependency);
    }

    [BurstCompile]
    private partial struct ActorAnimateJob : IJobEntity
    {
        [ReadOnly]
        public ComponentLookup<AnimatorActorPartComponent> PartComponentLookup;
        public float DeltaTime;

        [BurstCompile]
        public void Execute(
            [ChunkIndexInQuery] int sortKey,
            in DynamicBuffer<AnimationBuffer> animations,
            ref DynamicBuffer<AnimatorActorParametersBuffer> parameters,
            in DynamicBuffer<AnimatorActorPartBufferComponent> parts,
            ref DynamicBuffer<AnimatorActorLayerBuffer> layers,
            in DynamicBuffer<LayerStateBuffer> states,
            in DynamicBuffer<StateTransitionBuffer> transitions,
            in DynamicBuffer<TransitionCondtionBuffer> conditions
            )
        {
            NativeArray<AnimationBuffer> _animations = animations.AsNativeArray();
            NativeArray<AnimatorActorPartBufferComponent> _parts = parts.AsNativeArray();
            NativeArray<LayerStateBuffer> _states = states.AsNativeArray();
            NativeArray<StateTransitionBuffer> _transitions = transitions.AsNativeArray();
            NativeArray<TransitionCondtionBuffer> _conditions = conditions.AsNativeArray();
            for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
            {
                var layer = layers[layerIndex];
                ProcessLayer(
                    sortKey,
                    ref layer,
                    _animations,
                    ref parameters,
                    _parts,
                    _states,
                    _transitions,
                    _conditions,
                    DeltaTime);
                layers[layerIndex] = layer;
            }
            _animations.Dispose();
            _parts.Dispose();
            _states.Dispose();
            _transitions.Dispose();
            _conditions.Dispose();
        }

        [BurstCompile]
        private void ProcessLayer(
            int sortKey,
            ref AnimatorActorLayerBuffer layer,
            NativeArray<AnimationBuffer> animations,
            ref DynamicBuffer<AnimatorActorParametersBuffer> parameters,
            NativeArray<AnimatorActorPartBufferComponent> parts,
            NativeArray<LayerStateBuffer> states,
            NativeArray<StateTransitionBuffer> transitions,
            NativeArray<TransitionCondtionBuffer> conditions,
            float deltaTime
            )
        {
            var _layer = new AnimatorActorLayerBuffer(layer);
            bool newTransitionFound = false;
            var newTransition = new StateTransitionBuffer();
            if (_layer.IsInTransition) // if alreay in transtion then continue the transition
            {
                if (_layer.TransitionTimer < 0) // transition completed, make changes
                {
                    CompleteTransition(ref layer);
                }
                else // transition still has time
                {

                }
            }
            // check parameters conditions matching for new transition if not already in transition
            else
            {
                foreach (var _transition in transitions)
                {
                    if (_transition.StateId == _layer.CurrentStateId)
                    {
                        bool allConditionsMetForThisTransition = false;
                        foreach (var condition in conditions)
                        {
                            allConditionsMetForThisTransition = false;
                            if (condition.TransitionId == _transition.Id)
                            {
                                allConditionsMetForThisTransition = CheckConditionMeet(condition, ref parameters);
                            }
                        }
                        if (allConditionsMetForThisTransition)
                        {
                            newTransitionFound = true;
                            newTransition = _transition;
                            break;
                        }
                    }
                }
            }
            if (newTransitionFound && !_layer.IsInTransition) // if new transition found and current transition ended or was missing register transitioning
            {
                SetTransition(ref _layer, newTransition, states);
            }

            // update layer info
            layer = _layer;
        }

        [BurstCompile]
        private void UpdateTransition(ref AnimatorActorLayerBuffer layer)
        {
            float currentTransitionTime = layer.TransitionTimer - layer.TransitionDuration;
            // calculate current animation time
            float exitTime = layer.ExitPercentage * layer.CurrentAnimationLength;
            float transitionDuration = (layer.FixedDuration)?layer.TransitionDuration:layer.TransitionDuration * layer.CurrentAnimationLength;
            float offsetTime = layer.NextAnimationLength * layer.TransitionOffsetPercentage;
            float newCurrentAnimationTime = exitTime;

            // calculate transition animation time
        }

        [BurstCompile]
        private void SetTransition(ref AnimatorActorLayerBuffer layer, StateTransitionBuffer newTransition, NativeArray<LayerStateBuffer> states)
        {
            layer.IsInTransition = true;
            layer.CurrentStateId = newTransition.Id;
            var newState = new LayerStateBuffer();
            foreach (var state in states)
            {
                if (state.Id == newTransition.Id)
                {
                    newState = state;
                    break;
                }
            }
            layer.CurrentStateSpeed = newState.Id;
            layer.CurrentAnimationId = newState.AnimationClipId;
            layer.CurrentAnimationTime = newState.AnimationLength;
            layer.CurrentAnimationIsLooped = newState.AnimationLooped;
            layer.TransitionTimer = 0f;
            layer.ExitPercentage = 0f;
            layer.FixedDuration = false;
            layer.TransitionDuration = 0f;
            layer.TransitionOffsetPercentage = 0f;
            layer.NextStateId = 0;
            layer.NextStateSpeed = 0f;
            layer.NextAnimationId = 0;
            layer.NextAnimationTime = 0f;
            layer.NextAnimationIsLooped = false;
        }

        [BurstCompile]
        private void CompleteTransition(ref AnimatorActorLayerBuffer layer)
        {
            layer.IsInTransition = false;
            layer.CurrentStateId = layer.NextStateId;
            layer.CurrentStateSpeed = layer.NextStateSpeed;
            layer.CurrentAnimationId = layer.NextAnimationId;
            layer.CurrentAnimationTime = layer.NextAnimationTime;
            layer.CurrentAnimationIsLooped = layer.NextAnimationIsLooped;
            layer.TransitionTimer = 0f;
            layer.ExitPercentage = 0f;
            layer.FixedDuration = false;
            layer.TransitionDuration = 0f;
            layer.TransitionOffsetPercentage = 0f;
            layer.NextStateId = 0;
            layer.NextStateSpeed = 0f;
            layer.NextAnimationId = 0;
            layer.NextAnimationTime = 0f;
            layer.NextAnimationIsLooped = false;
        }

        [BurstCompile]
        private bool CheckConditionMeet(TransitionCondtionBuffer condition, ref DynamicBuffer<AnimatorActorParametersBuffer> parameters)
        {
            bool result = false;
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.ParameterName == condition.Parameter)
                {
                    switch (condition.Mode)
                    {
                        case AnimatorTransitionConditionMode.If:
                            if (parameter.Type == UnityEngine.AnimatorControllerParameterType.Bool)
                            {
                                if (parameter.BoolValue)
                                {
                                    return true;
                                }
                            }
                            if (parameter.Type == UnityEngine.AnimatorControllerParameterType.Trigger)
                            {
                                if (parameter.BoolValue)
                                {
                                    parameter.BoolValue = false;
                                    parameters[i] = parameter;
                                    return true;
                                }
                            }
                            break;
                        case AnimatorTransitionConditionMode.IfNot:
                            if (parameter.Type == UnityEngine.AnimatorControllerParameterType.Bool)
                            {
                                if (!parameter.BoolValue)
                                {
                                    return true;
                                }
                            }
                            break;
                        case AnimatorTransitionConditionMode.Greater:
                            if (parameter.Type == UnityEngine.AnimatorControllerParameterType.Int || parameter.Type == UnityEngine.AnimatorControllerParameterType.Float)
                            {
                                if (parameter.NumericValue > condition.Treshold)
                                {
                                    return true;
                                }
                            }
                            break;
                        case AnimatorTransitionConditionMode.Less:
                            if (parameter.Type == UnityEngine.AnimatorControllerParameterType.Int || parameter.Type == UnityEngine.AnimatorControllerParameterType.Float)
                            {
                                if (parameter.NumericValue < condition.Treshold)
                                {
                                    return true;
                                }
                            }
                            break;
                        case AnimatorTransitionConditionMode.Equals:
                            if (parameter.Type == UnityEngine.AnimatorControllerParameterType.Int || parameter.Type == UnityEngine.AnimatorControllerParameterType.Float)
                            {
                                if (parameter.NumericValue == condition.Treshold)
                                {
                                    return true;
                                }
                            }
                            break;
                        case AnimatorTransitionConditionMode.NotEqual:
                            if (parameter.Type == UnityEngine.AnimatorControllerParameterType.Int || parameter.Type == UnityEngine.AnimatorControllerParameterType.Float)
                            {
                                if (parameter.NumericValue != condition.Treshold)
                                {
                                    return true;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return result;
        }
    }



    //[BurstCompile]
    //private void ProcessLayer(
    //    int sortKey,
    //    ref AnimatorActorLayerBuffer layer,
    //    NativeArray<AnimatorActorPartBufferComponent> actorParts,
    //    NativeArray<AnimatorActorParametersBuffer> actorParameters,
    //    float deltaTime)
    //{
    //    // finding animation clip id
    //    int currentAnimationId = -1;
    //    bool animationFound = false;
    //    foreach (var state in States)
    //    {
    //        if (state.AnimatorInstanceId == _animatorInstatnceId && state.Id == layer.CurrentStateId)
    //        {
    //            currentAnimationId = state.AnimationClipId;
    //            break;
    //        }
    //    }
    //    AnimationBuffer animationClip = new AnimationBuffer();
    //    foreach (var animation in Animations)
    //    {
    //        if (animation.AnimatorInstanceId == _animatorInstatnceId && animation.Id == currentAnimationId)
    //        {
    //            animationClip = animation;
    //            animationFound = true;
    //            break;
    //        }
    //    }
    //    if (!animationFound) // animation clip somehow not found, returning it
    //    {
    //        return;
    //    }

    //    // check parameters for conditions



    //    // animation found let's animate
    //    // manage timers
    //    var animationDuration = animationClip.Length;
    //    var looped = animationClip.Looped;
    //    var currentTimer = layer.CurrentAnimationTime;
    //    var loopEnded = false;
    //    if (currentTimer > animationDuration)
    //    {
    //        if (looped)
    //        {
    //            currentTimer = currentTimer % animationDuration;
    //        }
    //        else
    //        {
    //            currentTimer = animationDuration;
    //            loopEnded = true;
    //        }
    //    }
    //    // loop over paths
    //    foreach (var part in actorParts)
    //    {
    //        // positions
    //        float3 firstPosition = float3.zero;
    //        float3 secondPosition = float3.zero;
    //        bool firstPositionFound = false;
    //        bool secondPositionFound = false;
    //        float firstPositionTime = -1f;
    //        float secondPositionTime = -1f;
    //        foreach (var position in Positions)
    //        {
    //            if (position.AnimationId == currentAnimationId && part.Path == position.Path)
    //            {
    //                if (currentTimer <= position.Time)
    //                {
    //                    if (!firstPositionFound)
    //                    {
    //                        firstPosition = position.Value;
    //                        firstPositionTime = position.Time;
    //                        firstPositionFound = true;
    //                    }
    //                    else
    //                    {
    //                        if (firstPositionTime > position.Time)
    //                        {
    //                            firstPosition = position.Value;
    //                            firstPositionTime = position.Time;
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    if (!secondPositionFound)
    //                    {
    //                        secondPosition = position.Value;
    //                        secondPositionTime = position.Time;
    //                        secondPositionFound = true;
    //                    }
    //                    else
    //                    {
    //                        if (secondPositionTime < position.Time)
    //                        {
    //                            secondPosition = position.Value;
    //                            secondPositionTime = position.Time;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        // rotations
    //        quaternion firstRotation = quaternion.identity;
    //        quaternion secondRotation = quaternion.identity;
    //        bool firstRotationFound = false;
    //        bool secondRotationFound = false;
    //        float firstRotationTime = -1f;
    //        float secondRotationTime = -1f;
    //        foreach (var rotation in Rotations)
    //        {
    //            if (rotation.AnimationId == currentAnimationId && part.Path == rotation.Path)
    //            {
    //                if (currentTimer <= rotation.Time)
    //                {
    //                    if (!firstRotationFound)
    //                    {
    //                        firstRotation = rotation.Value;
    //                        firstRotationTime = rotation.Time;
    //                        firstRotationFound = true;
    //                    }
    //                    else
    //                    {
    //                        if (firstRotationTime > rotation.Time)
    //                        {
    //                            firstRotation = rotation.Value;
    //                            firstRotationTime = rotation.Time;
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    if (!secondRotationFound)
    //                    {
    //                        secondRotation = rotation.Value;
    //                        secondRotationTime = rotation.Time;
    //                        secondRotationFound = true;
    //                    }
    //                    else
    //                    {
    //                        if (secondRotationTime < rotation.Time)
    //                        {
    //                            secondRotation = rotation.Value;
    //                            secondRotationTime = rotation.Time;
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        // get current data from local transform
    //        Entity partEntity = part.Value;
    //        RefRO<LocalTransform> partLocaltTransform = PartComponentLookup.GetRefRO(partEntity);
    //        float3 newPosition = partLocaltTransform.ValueRO.Position;
    //        quaternion newRotation = partLocaltTransform.ValueRO.Rotation;
    //        float scale = partLocaltTransform.ValueRO.Scale;

    //        // calculcate rates
    //        if (loopEnded)
    //        {
    //            if (firstPositionFound)
    //            {
    //                newPosition = firstPosition;
    //            }
    //            if (firstRotationFound)
    //            {
    //                newRotation = firstRotation;
    //            }
    //        }
    //        else
    //        {
    //            if (secondPositionFound)
    //            {
    //                float rate = (currentTimer - firstPositionTime) / (secondPositionTime - firstPositionTime);
    //                newPosition = math.lerp(firstPosition, secondPosition, rate);
    //            }
    //            if (secondRotationFound)
    //            {
    //                float rate = (currentTimer - firstRotationTime) / (secondRotationTime - firstRotationTime);
    //                newRotation = math.slerp(firstRotation, secondRotation, rate);
    //            }
    //        }

    //        // apply
    //        ParallelWriter.SetComponent(sortKey, partEntity, new LocalTransform
    //        {
    //            Position = newPosition,
    //            Rotation = newRotation,
    //            Scale = scale
    //        });
    //    }
    //    currentTimer += deltaTime;
    //    layer.CurrentAnimationTime = currentTimer;
    //}
}