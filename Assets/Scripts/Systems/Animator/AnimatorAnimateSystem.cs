using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct AnimatorAnimateSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimatorActorComponent>();
        state.RequireForUpdate<AnimatorActorPartComponent>();
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
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        var parallelWriter = ecb.AsParallelWriter();
        float deltaTime = SystemAPI.Time.DeltaTime;

        state.Dependency = new ActorAnimateJob
        {
            ParallelWriter = parallelWriter,
            DeltaTime = deltaTime
        }.ScheduleParallel(acrtorsQuery, state.Dependency);

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    private partial struct ActorAnimateJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ParallelWriter;
        public float DeltaTime;

        [BurstCompile]
        public void Execute(
            [ChunkIndexInQuery] int sortKey,
            ref DynamicBuffer<AnimatorActorParametersBuffer> parameters,
            in DynamicBuffer<AnimatorActorPartBufferComponent> parts,
            ref DynamicBuffer<AnimatorActorLayerBuffer> layers,
            in DynamicBuffer<LayerStateBuffer> states,
            in DynamicBuffer<StateTransitionBuffer> transitions,
            in DynamicBuffer<TransitionCondtionBuffer> conditions
            )
        {
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
                    ref parameters,
                    _parts,
                    _states,
                    _transitions,
                    _conditions,
                    DeltaTime);
                layers[layerIndex] = layer;
            }
            _parts.Dispose();
            _states.Dispose();
            _transitions.Dispose();
            _conditions.Dispose();
        }

        [BurstCompile]
        private void ProcessLayer(
            int sortKey,
            ref AnimatorActorLayerBuffer layer,
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
            if (!_layer.IsInTransition) // if no transition thet check parameters conditions matching for new transition
            {
                foreach (var _transition in transitions)
                {
                    if (_transition.StateId == _layer.CurrentStateId)
                    {
                        bool allConditionsMet = true;
                        bool conditionMet = false;
                        foreach (var condition in conditions)
                        {
                            if (condition.TransitionId == _transition.Id)
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
            else // already in transition
            {
                if (_layer.FirstOffsetTimer <= 0f && _layer.TransitionTimer <= 0f) // check if timers are completed
                {
                    PerfromTransition(ref _layer); // transition fully completed
                }
            }
            if (!_layer.IsInTransition && newTransitionFound) // new transition found
            {
                SetNewTransition(ref _layer, newTransition, states);
            }
            // clamp timer
            ClampTimers(ref _layer);
            // set parts' component
            SetPartsComponents(sortKey, ref _layer, parts);
            // shift timer
            AddToTimers(ref _layer, deltaTime);
            // update layer info
            layer = _layer;
        }

        [BurstCompile]
        private void SetPartsComponents(int sortKey, ref AnimatorActorLayerBuffer layer, NativeArray<AnimatorActorPartBufferComponent> parts)
        {
            float transitionRate = -1f;
            layer.TransitionRate = 0;
            if (layer.IsInTransition && layer.FirstOffsetTimer <= 0f)
            {
                transitionRate = (layer.TransitionDuration - layer.TransitionTimer) / layer.TransitionDuration;
                transitionRate = math.clamp(transitionRate, 0f, 1f);
                layer.TransitionRate = transitionRate;
            }
            foreach (var part in parts)
            {
                ParallelWriter.AddComponent(sortKey, part.Value, new AnimatorActorPartComponent
                {
                   CurrentAnimationClipId = layer.CurrentAnimationId,
                   CurrentAnimationTime = layer.CurrentAnimationTime,
                   NextAnimationClipId = layer.NextAnimationId,
                   NextAnimationTime = layer.NextAnimationTime,
                   TransitionRate = transitionRate,
                   Method = layer.Method
                });
            }
        }

        [BurstCompile]
        private void SetNewTransition(ref AnimatorActorLayerBuffer layer, StateTransitionBuffer newTransition, NativeArray<LayerStateBuffer> states)
        {
            var newState = new LayerStateBuffer();
            foreach (var _state in states)
            {
                if (newTransition.DestinationStateId == _state.Id)
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
}