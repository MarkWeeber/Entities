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
             AnimatorActorPartBufferComponent,
             AnimatorActorLayerBuffer>()
            .Build();

        if (acrtorsQuery.CalculateEntityCount() < 1)
        {
            return;
        }

        if (SystemAPI.TryGetSingletonBuffer<AnimatorBuffer>(out DynamicBuffer<AnimatorBuffer> animatorBuffers))
        {
            NativeArray<LayerStateBuffer> states = SystemAPI.GetSingletonBuffer<LayerStateBuffer>().AsNativeArray();
            NativeArray<StateTransitionBuffer> transitions = SystemAPI.GetSingletonBuffer<StateTransitionBuffer>().AsNativeArray();
            NativeArray<TransitionCondtionBuffer> conditions = SystemAPI.GetSingletonBuffer<TransitionCondtionBuffer>().AsNativeArray();
            NativeArray<AnimationBlobBuffer> animationBlob = SystemAPI.GetSingletonBuffer<AnimationBlobBuffer>().AsNativeArray();

            //var ecb = new EntityCommandBuffer(Allocator.TempJob);
            //var parallelWriter = ecb.AsParallelWriter();
            float deltaTime = SystemAPI.Time.DeltaTime;
            localTransformLookup.Update(ref state);

            state.Dependency = new ProcessAnimatorsJob
            {
                States = states,
                Transitions = transitions,
                Conditions = conditions,
                AnimationBlob = animationBlob,
                LocalTransformLookup = localTransformLookup,
                DeltaTime = deltaTime
            }.ScheduleParallel(acrtorsQuery, state.Dependency);


            animationBlob.Dispose();
            states.Dispose();
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
        public NativeArray<TransitionCondtionBuffer> Conditions;
        [ReadOnly]
        public NativeArray<AnimationBlobBuffer> AnimationBlob;
        [ReadOnly]
        public ComponentLookup<LocalTransform> LocalTransformLookup;
        public float DeltaTime;
        [BurstCompile]
        public void Execute(
            [ChunkIndexInQuery] int sortKey,
            ref DynamicBuffer<AnimatorActorParametersBuffer> parameters,
            ref DynamicBuffer<AnimatorActorPartBufferComponent> parts,
            ref DynamicBuffer<AnimatorActorLayerBuffer> layers,
            RefRO<AnimatorActorComponent> animatorActorComponent
            )
        {
            for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
            {
                var layer = layers[layerIndex];
                ProcessLayer(
                    sortKey,
                    ref layer,
                    ref parameters,
                    ref parts,
                    DeltaTime,
                    animatorActorComponent.ValueRO.AnimatorId);
                layers[layerIndex] = layer;
            }
        }

        [BurstCompile]
        private void ProcessLayer(
            int sortKey,
            ref AnimatorActorLayerBuffer layer,
            ref DynamicBuffer<AnimatorActorParametersBuffer> parameters,
            ref DynamicBuffer<AnimatorActorPartBufferComponent> parts,
            float deltaTime,
            int animatorInstanceId
            )
        {
            var _layer = new AnimatorActorLayerBuffer(layer);
            bool newTransitionFound = false;
            var newTransition = new StateTransitionBuffer();
            if (!_layer.IsInTransition) // if no transition thet check parameters conditions matching for new transition
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

        [BurstCompile]
        private void AnimateParts(
            int sortKey,
            ref AnimatorActorLayerBuffer layer,
            ref DynamicBuffer<AnimatorActorPartBufferComponent> parts)
        {
            float transitionRate = -1f;
            layer.TransitionRate = 0;
            if (layer.IsInTransition && layer.FirstOffsetTimer <= 0f)
            {
                transitionRate = (layer.TransitionDuration - layer.TransitionTimer) / layer.TransitionDuration;
                transitionRate = math.clamp(transitionRate, 0f, 1f);
                layer.TransitionRate = transitionRate;
            }
            int currentAnimationId = layer.CurrentAnimationId;
            int nextAnimationId = layer.NextAnimationId;
            bool isInTransition = layer.IsInTransition;
            int currentAnimIndex = 0;
            int nextAnimIndex = 0;
            bool currentFound = false;
            bool nextFound = false;
            for (int i = 0; i < AnimationBlob.Length; i++)
            {
                var animationBlob = AnimationBlob[i];
                if (animationBlob.Id == currentAnimationId)
                {
                    currentAnimIndex = i;
                    currentFound = true;
                }
                if (isInTransition && animationBlob.Id == nextAnimationId)
                {
                    nextAnimIndex = i;
                    nextFound = true;
                }
                if (currentFound)
                {
                    if (isInTransition)
                    {
                        if (nextFound)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //ref RotationsPool currentRotationsPool = ref AnimationBlob[currentAnimIndex].Rotations.Value;
            //ref RotationsPool nextRotationsPool = ref AnimationBlob[nextAnimIndex].Rotations.Value;
            //ref PositionsPool currentPositionsPool = ref AnimationBlob[currentAnimIndex].Position.Value;
            //ref PositionsPool nextPositionsPool = ref AnimationBlob[nextAnimIndex].Position.Value;
            //for (int i = 0; i < parts.Length; i++)
            //{
            //    var part = parts[i];
            //    var newLocalTransform = ObtainPartAnimationValue(
            //        sortKey,
            //        layer,
            //        part,
            //        ref currentRotationsPool,
            //        ref nextRotationsPool,
            //        ref currentPositionsPool,
            //        ref nextPositionsPool);
            //    part.SetNewLocalTransform = true;
            //    part.SetPosition = newLocalTransform.Position;
            //    part.SetRotation = newLocalTransform.Rotation;
            //    part.SetScale = newLocalTransform.Scale;
            //    parts[i] = part;
            //}
        }

        [BurstCompile]
        private LocalTransform ObtainPartAnimationValue(
            int sortKey,
            AnimatorActorLayerBuffer layer,
            AnimatorActorPartBufferComponent part,
            ref RotationsPool currentRotationsPool,
            ref RotationsPool nextRotationsPool,
            ref PositionsPool currentPositionsPool,
            ref PositionsPool nextPositionsPool)
        {
            int currentAnimationId = layer.CurrentAnimationId;
            float currentAnimationTime = layer.CurrentAnimationTime;
            var localTransform = LocalTransformLookup.GetRefRO(part.Value);
            float3 setPosition = localTransform.ValueRO.Position;
            quaternion setRotation = localTransform.ValueRO.Rotation;
            float setScale = localTransform.ValueRO.Scale;

            // obtain first animation values
            ObtainAnimationValues(
                ref setPosition,
                ref setRotation,
                currentAnimationTime,
                currentAnimationId,
                part,
                layer.Method,
                ref currentRotationsPool,
                ref currentPositionsPool);

            // check if transition exists
            float transitionRate = layer.TransitionRate;
            if (transitionRate >= 0)
            {
                int nextAnimationId = layer.NextAnimationId;
                float nextAnimationTime = layer.NextAnimationTime;
                float3 nextPosition = localTransform.ValueRO.Position;
                quaternion nextRotation = localTransform.ValueRO.Rotation;
                ObtainAnimationValues(
                    ref nextPosition,
                    ref nextRotation,
                    nextAnimationTime,
                    nextAnimationId,
                    part,
                    layer.Method,
                    ref nextRotationsPool,
                    ref nextPositionsPool);
                switch (layer.Method)
                {
                    case PartsAnimationMethod.Lerp:
                        setPosition = math.lerp(setPosition, nextPosition, transitionRate);
                        setRotation = math.slerp(setRotation, nextRotation, transitionRate);
                        break;
                    case PartsAnimationMethod.Lean:
                        setPosition = CustomMath.Lean(setPosition, nextPosition, transitionRate);
                        setRotation = CustomMath.Lean(setRotation, nextRotation, transitionRate);
                        break;
                    case PartsAnimationMethod.SmoothStep:
                        setPosition = CustomMath.SmoothStep(setPosition, nextPosition, transitionRate);
                        setRotation = CustomMath.SmoothStep(setRotation, nextRotation, transitionRate);
                        break;
                    default:
                        break;
                }
            }

            return new LocalTransform
            {
                Position = setPosition,
                Rotation = setRotation,
                Scale = setScale
            };
        }

        [BurstCompile]
        private void ObtainAnimationValues(
            ref float3 position,
            ref quaternion rotation,
            float animationTime,
            int animationId,
            AnimatorActorPartBufferComponent part,
            PartsAnimationMethod method,
            ref RotationsPool rotationsPool,
            ref PositionsPool positionsPool)
        {
            bool firstPosFound = false;
            bool secondPosFound = false;
            float3 firstPos = float3.zero;
            float3 secondPos = float3.zero;
            float firstPosTime = 0f;
            float secondPosTime = 0f;
            for (int i = 0; i < positionsPool.Positions.Length; i++)
            {
                //var pos = positionsPool.Positions[i];
                //if (pos.Path == part.Path && pos.AnimationId == animationId)
                //{
                //    if (pos.Time <= animationTime)
                //    {
                //        firstPosFound = true;
                //        firstPosTime = pos.Time;
                //        firstPos = pos.Value;
                //    }
                //    if (pos.Time > animationTime)
                //    {
                //        secondPosFound = true;
                //        secondPosTime = pos.Time;
                //        secondPos = pos.Value;
                //        break;
                //    }
                //}
            }
            bool firstRotFound = false;
            bool secondRotFound = false;
            quaternion firstRot = quaternion.identity;
            quaternion secondRot = quaternion.identity;
            float firstRotTime = 0f;
            float secondRotTime = 0f;
            for (int i = 0; i < rotationsPool.Rotations.Length; i++)
            {
                //var rot = rotationsPool.Rotations[i];
                //if (rot.Path == part.Path && rot.AnimationId == animationId)
                //{
                //    if (rot.Time <= animationTime)
                //    {
                //        firstRotFound = true;
                //        firstRotTime = rot.Time;
                //        firstRot = rot.Value;
                //    }
                //    if (rot.Time > animationTime)
                //    {
                //        secondRotFound = true;
                //        secondRotTime = rot.Time;
                //        secondRot = rot.Value;
                //        break;
                //    }
                //}
            }

            if (secondPosFound && firstPosFound)
            {
                float rate = (animationTime - firstPosTime) / (secondPosTime - firstPosTime);
                switch (method)
                {
                    case PartsAnimationMethod.Lerp:
                        position = math.lerp(firstPos, secondPos, rate);
                        break;
                    case PartsAnimationMethod.Lean:
                        position = CustomMath.Lean(firstPos, secondPos, rate);
                        break;
                    case PartsAnimationMethod.SmoothStep:
                        position = CustomMath.SmoothStep(firstPos, secondPos, rate);
                        break;
                    default:
                        break;
                }

            }
            if (firstPosFound && !secondPosFound)
            {
                position = firstPos;
            }
            if (secondRotFound && firstRotFound)
            {
                float rate = (animationTime - firstRotTime) / (secondRotTime - firstRotTime);
                switch (method)
                {
                    case PartsAnimationMethod.Lerp:
                        rotation = math.slerp(firstRot, secondRot, rate);
                        break;
                    case PartsAnimationMethod.Lean:
                        rotation = CustomMath.Lean(firstRot, secondRot, rate);
                        break;
                    case PartsAnimationMethod.SmoothStep:
                        rotation = CustomMath.SmoothStep(firstRot, secondRot, rate);
                        break;
                    default:
                        break;
                }

            }
            if (firstRotFound && !secondRotFound)
            {
                rotation = firstRot;
            }
        }
    }
}