using CustomUtils;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct AnimatorAnimateSystem : ISystem
{
    private ComponentLookup<LocalTransform> localTransformLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimatorActorComponent>();
        localTransformLookup = state.GetComponentLookup<LocalTransform>(true);
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
             AnimatorActorComponent,
             AnimatorActorParametersBuffer,
             AnimatorActorLayerBuffer>()
            .Build();

        if (acrtorsQuery.CalculateEntityCount() < 1)
        {
            return;
        }

        if (SystemAPI.TryGetSingletonBuffer<AnimatorBuffer>(out DynamicBuffer<AnimatorBuffer> animatorBuffers))
        {
            NativeArray<LayerStateBuffer> states = SystemAPI.GetSingletonBuffer<LayerStateBuffer>().AsNativeArray();
            NativeArray<AnyStateTransitionBuffer> anyStateTransitions = SystemAPI.GetSingletonBuffer<AnyStateTransitionBuffer>().AsNativeArray();
            NativeArray<StateTransitionBuffer> transitions = SystemAPI.GetSingletonBuffer<StateTransitionBuffer>().AsNativeArray();
            NativeArray<TransitionCondtionBuffer> conditions = SystemAPI.GetSingletonBuffer<TransitionCondtionBuffer>().AsNativeArray();
            NativeArray<AnimationBlobBuffer> animationBlob = SystemAPI.GetSingletonBuffer<AnimationBlobBuffer>().AsNativeArray();

            float deltaTime = SystemAPI.Time.DeltaTime;
            localTransformLookup.Update(ref state);

            var processAnimatorJobHandle = new ProcessAnimatorsJob
            {
                States = states,
                Transitions = transitions,
                AnyStateTransitions = anyStateTransitions,
                Conditions = conditions,
                LocalTransformLookup = localTransformLookup,
                DeltaTime = deltaTime
            }.ScheduleParallel(acrtorsQuery, state.Dependency);

            state.Dependency = new ProcessAnimationEventsJob
            {
                AnimationBlob = animationBlob
            }.ScheduleParallel(acrtorsQuery, processAnimatorJobHandle);

            animationBlob.Dispose();
            states.Dispose();
            anyStateTransitions.Dispose();
            transitions.Dispose();
            conditions.Dispose();
        }
    }

    [BurstCompile]
    private partial struct ProcessAnimatorsJob : IJobEntity
    {
        [ReadOnly]
        public NativeArray<LayerStateBuffer> States;
        [ReadOnly]
        public NativeArray<StateTransitionBuffer> Transitions;
        [ReadOnly]
        public NativeArray<AnyStateTransitionBuffer> AnyStateTransitions;
        [ReadOnly]
        public NativeArray<TransitionCondtionBuffer> Conditions;
        [ReadOnly]
        public ComponentLookup<LocalTransform> LocalTransformLookup;
        public float DeltaTime;
        [BurstCompile]
        public void Execute(
            ref DynamicBuffer<AnimatorActorParametersBuffer> parameters,
            ref DynamicBuffer<AnimatorActorLayerBuffer> layers,
            RefRO<AnimatorActorComponent> animatorActorComponent
            )
        {
            for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
            {
                var layer = layers[layerIndex];
                ProcessLayer(
                    ref layer,
                    ref parameters,
                    DeltaTime,
                    animatorActorComponent.ValueRO.AnimatorId);
                layers[layerIndex] = layer;
            }
        }

        [BurstCompile]
        private void ProcessLayer(
            ref AnimatorActorLayerBuffer layer,
            ref DynamicBuffer<AnimatorActorParametersBuffer> parameters,
            float deltaTime,
            int animatorInstanceId
            )
        {
            var _layer = new AnimatorActorLayerBuffer(layer);
            bool newTransitionFound = false;
            var newTransition = new StateTransitionBuffer();
            if (!_layer.IsInTransition) // if no transition thet check parameters conditions matching for new transition
            {
                foreach (var _anyStateTransition in AnyStateTransitions)
                {
                    if (_anyStateTransition.AnimatorInstanceId == animatorInstanceId)
                    {
                        bool allConditionsMet = true;
                        bool conditionMet = false;
                        foreach (var condition in Conditions)
                        {
                            if (condition.TransitionId == _anyStateTransition.Id && condition.AnimatorInstanceId == animatorInstanceId)
                            {
                                conditionMet = CheckConditionMeet(condition, ref parameters);
                                if (!conditionMet)
                                {
                                    allConditionsMet = false;
                                }
                            }
                        }
                        if (allConditionsMet)
                        {
                            newTransitionFound = true;
                            newTransition = new StateTransitionBuffer(_anyStateTransition);
                            break;
                        }
                    }
                }
                if (!newTransitionFound)
                {
                    foreach (var _transition in Transitions)
                    {
                        if (_transition.StateId == _layer.CurrentStateId && _transition.AnimatorInstanceId == animatorInstanceId)
                        {
                            bool allConditionsMet = true;
                            bool conditionMet = false;
                            foreach (var condition in Conditions)
                            {
                                if (condition.TransitionId == _transition.Id && condition.AnimatorInstanceId == animatorInstanceId)
                                {
                                    conditionMet = CheckConditionMeet(condition, ref parameters);
                                    if (!conditionMet)
                                    {
                                        allConditionsMet = false;
                                    }
                                }
                            }
                            if (allConditionsMet)
                            {
                                newTransitionFound = true;
                                newTransition = _transition;
                                break;
                            }
                        }
                    }
                }
            }
            else // already in transition
            {
                if (_layer.FirstOffsetTimer <= 0f && _layer.TransitionTimer <= 0f) // check if timers are completed
                {
                    PerfromTransition(ref _layer); // transition fully completed
                }
            }
            if (!_layer.IsInTransition && newTransitionFound) // new transition found
            {
                SetNewTransition(ref _layer, newTransition, animatorInstanceId);
            }
            // clamp timer
            ClampTimers(ref _layer);
            // set parts' component
            //AnimateParts(sortKey, ref _layer, ref parts);
            // shift timer
            AddToTimers(ref _layer, deltaTime);
            // update layer info
            layer = _layer;
        }

        [BurstCompile]
        private void SetNewTransition(ref AnimatorActorLayerBuffer layer, StateTransitionBuffer newTransition, int animatorInstanceId)
        {
            var newState = new LayerStateBuffer();
            foreach (var _state in States)
            {
                if (_state.AnimatorInstanceId == animatorInstanceId && newTransition.DestinationStateId == _state.Id)
                {
                    newState = _state;
                    break;
                }
            }
            // transition info
            float transitionDuration = (newTransition.FixedDuration) ?
                newTransition.TransitionDuration :
                newTransition.TransitionDuration * newState.AnimationLength / newState.Speed;
            layer.IsInTransition = true; // main transition switch
            layer.TransitionDuration = transitionDuration; // actual transition duration
            layer.TransitionTimer = transitionDuration; // actual transition timer
            layer.TransitionRate = 0;
            layer.FirstOffsetTimer = newTransition.ExitTime * layer.CurrentAnimationLength / layer.CurrentStateSpeed; // start offset timer
            layer.SecondAnimationOffset = newTransition.TransitionOffset * layer.NextAnimationLength; // offset for second animation start

            // second state and animation info
            layer.NextStateId = newState.Id;
            layer.NextStateSpeed = newState.Speed;
            layer.NextAnimationBlobIndex = newState.AnimationBlobAssetIndex;
            layer.NextAnimationId = newState.AnimationClipId;
            layer.NextAnimationTime = layer.SecondAnimationOffset; // time needed in transitioning animation
            layer.NextAnimationLength = newState.AnimationLength;
            layer.NextAnimationIsLooped = newState.AnimationLooped;
        }

        [BurstCompile]
        private bool CheckConditionMeet(TransitionCondtionBuffer condition, ref DynamicBuffer<AnimatorActorParametersBuffer> parameters)
        {
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
            return false;
        }

        [BurstCompile]
        private void ClampTimers(ref AnimatorActorLayerBuffer layer)
        {
            if (layer.CurrentAnimationTime > layer.CurrentAnimationLength)
            {
                if (layer.CurrentAnimationIsLooped)
                {
                    layer.CurrentAnimationTime = layer.CurrentAnimationTime % layer.CurrentAnimationLength;
                }
                else
                {
                    layer.CurrentAnimationTime = layer.CurrentAnimationLength;
                }
            }
            if (layer.IsInTransition)
            {
                if (layer.NextAnimationTime > layer.NextAnimationLength)
                {
                    if (layer.NextAnimationIsLooped)
                    {
                        layer.NextAnimationTime = layer.NextAnimationTime % layer.NextAnimationLength;
                    }
                    else
                    {
                        layer.NextAnimationTime = layer.NextAnimationLength;
                    }
                }
            }
        }

        [BurstCompile]
        private void PerfromTransition(ref AnimatorActorLayerBuffer layer)
        {
            // current state and animation info
            layer.CurrentStateId = layer.NextStateId;
            layer.CurrentStateSpeed = layer.NextStateSpeed;
            layer.CurrentAnimationBlobIndex = layer.NextAnimationBlobIndex;
            layer.CurrentAnimationId = layer.NextAnimationId;
            layer.CurrentAnimationTime = layer.NextAnimationTime; // time needed for animation
            layer.CurrentAnimationLength = layer.NextAnimationLength;
            layer.CurrentAnimationIsLooped = layer.NextAnimationIsLooped;

            // transition info
            layer.IsInTransition = false; // main transition switch
            layer.TransitionDuration = 0f; // actual transition duration
            layer.TransitionTimer = 0f; // actual transition timer
            layer.TransitionRate = 0;
            layer.FirstOffsetTimer = 0f; // start offset timer
            layer.SecondAnimationOffset = 0f; // offset for second animation start

            // second state and animation info
            layer.NextStateId = 0;
            layer.NextStateSpeed = 0f;
            layer.NextAnimationBlobIndex = -1;
            layer.NextAnimationId = 0;
            layer.NextAnimationTime = 0f; // time needed in transitioning animation
            layer.NextAnimationLength = 0f;
            layer.NextAnimationIsLooped = false;
        }

        [BurstCompile]
        private void AddToTimers(ref AnimatorActorLayerBuffer layer, float deltaTime)
        {
            layer.CurrentAnimationTime += deltaTime * layer.CurrentStateSpeed;
            if (layer.IsInTransition)
            {
                if (layer.FirstOffsetTimer > 0)
                {
                    layer.FirstOffsetTimer -= deltaTime;
                }
                if (layer.FirstOffsetTimer <= 0) // inside duration
                {
                    float exessiveTime = layer.FirstOffsetTimer * -1;
                    layer.NextAnimationTime += (deltaTime * layer.NextAnimationSpeed) + exessiveTime;
                    layer.TransitionTimer -= (exessiveTime + deltaTime);
                }
            }
        }
    }

    [BurstCompile]
    private partial struct ProcessAnimationEventsJob : IJobEntity
    {
        [ReadOnly]
        public NativeArray<AnimationBlobBuffer> AnimationBlob;
        [BurstCompile]
        private void Execute(in DynamicBuffer<AnimatorActorLayerBuffer> layers)
        {
            foreach (var layer in layers)
            {
                if (!layer.IsInTransition)
                {
                    var currentTime = layer.CurrentAnimationTime;
                    var currentAnimationBlobIndex = layer.CurrentAnimationBlobIndex;
                    var animationBlob = AnimationBlob[currentAnimationBlobIndex];
                    ref var events = ref animationBlob.AnimationEventsData.Value.EventsData;
                    for (int i = 0; i < events.Length; i++)
                    {
                        var time = events[i].Time;
                        if (time > currentTime)
                        {
                            Debug.Log($"Time: {time} Event: {events[i].EventName.ToString()}");
                        }
                    }
                }
            }
        }
    }
}