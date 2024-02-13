using System.Globalization;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct AnimatorAnimateSystem : ISystem
{
    private ComponentLookup<LocalTransform> localTransformLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimatorBuffer>();
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
        if (SystemAPI.TryGetSingletonBuffer<AnimatorBuffer>(out DynamicBuffer<AnimatorBuffer> animatorDB))
        {
            NativeArray<AnimatorBuffer> animators = animatorDB.AsNativeArray();
            NativeArray<AnimationBuffer> animations = SystemAPI.GetSingletonBuffer<AnimationBuffer>().AsNativeArray();
            NativeArray<AnimatorLayerBuffer> layers = SystemAPI.GetSingletonBuffer<AnimatorLayerBuffer>().AsNativeArray();
            NativeArray<LayerStateBuffer> states = SystemAPI.GetSingletonBuffer<LayerStateBuffer>().AsNativeArray();
            NativeArray<AnimationPositionBuffer> positions = SystemAPI.GetSingletonBuffer<AnimationPositionBuffer>().AsNativeArray();
            NativeArray<AnimationRotationBuffer> rotations = SystemAPI.GetSingletonBuffer<AnimationRotationBuffer>().AsNativeArray();
            NativeArray<StateTransitionBuffer> transitions = SystemAPI.GetSingletonBuffer<StateTransitionBuffer>().AsNativeArray();
            NativeArray<TransitionCondtionBuffer> conditions = SystemAPI.GetSingletonBuffer<TransitionCondtionBuffer>().AsNativeArray();

            EntityQuery actorPartsQuery = SystemAPI.QueryBuilder()
                .WithAll<
                AnimatorActorComponent,
                AnimatorActorParametersBuffer,
                AnimatorActorPartBufferComponent,
                AnimatorActorTransitionBuffer,
                AnimatorActorLayerBuffer>()
                .Build();

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
            localTransformLookup.Update(ref state);
            float deltaTime = SystemAPI.Time.DeltaTime;

            state.Dependency = new ActorAnimateJob
            {
                Animators = animators,
                Animations = animations,
                Positions = positions,
                Rotations = rotations,
                Conditions = conditions,
                Layers = layers,
                States = states,
                Transitions = transitions,
                LocalTransformLookup = localTransformLookup,
                ParallelWriter = parallelWriter,
                DeltaTime = deltaTime
            }.ScheduleParallel(actorPartsQuery, state.Dependency);

            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            animators.Dispose();
            animations.Dispose();
            layers.Dispose();
            states.Dispose();
            transitions.Dispose();
            conditions.Dispose();
        }
    }

    [BurstCompile]
    private partial struct ActorAnimateJob : IJobEntity
    {
        [ReadOnly]
        public NativeArray<AnimatorBuffer> Animators;
        [ReadOnly]
        public NativeArray<AnimationBuffer> Animations;
        [ReadOnly]
        public NativeArray<AnimatorLayerBuffer> Layers;
        [ReadOnly]
        public NativeArray<LayerStateBuffer> States;
        [ReadOnly]
        public NativeArray<AnimationPositionBuffer> Positions;
        [ReadOnly]
        public NativeArray<AnimationRotationBuffer> Rotations;
        [ReadOnly]
        public NativeArray<StateTransitionBuffer> Transitions;
        [ReadOnly]
        public NativeArray<TransitionCondtionBuffer> Conditions;
        [ReadOnly]
        public ComponentLookup<LocalTransform> LocalTransformLookup;
        public EntityCommandBuffer.ParallelWriter ParallelWriter;
        public float DeltaTime;
        private int _animatorInstatnceId;
        [BurstCompile]
        private void Execute(
            [ChunkIndexInQuery] int sortKey,
            RefRO<AnimatorActorComponent> animatorActor,
            DynamicBuffer<AnimatorActorParametersBuffer> actorParameters,
            DynamicBuffer<AnimatorActorPartBufferComponent> actorParts,
            DynamicBuffer<AnimatorActorTransitionBuffer> actorTransitions,
            DynamicBuffer<AnimatorActorLayerBuffer> actorLayers
        )
        {
            // getting animator id
            _animatorInstatnceId = animatorActor.ValueRO.AnimatorId;

            // loop each layer
            for (int layerIndex = 0; layerIndex < actorLayers.Length; layerIndex++)
            {
                var layer = actorLayers[layerIndex];
                AnimateLayer(sortKey, ref layer, ref actorParts, DeltaTime);
                actorLayers[layerIndex] = layer;
            }
        }

        [BurstCompile]
        private void AnimateLayer(
            int sortKey,
            ref AnimatorActorLayerBuffer layer,
            ref DynamicBuffer<AnimatorActorPartBufferComponent> actorParts,
            float deltaTime)
        {
            // finding animation clip id
            int animationClipId = -1;
            foreach (var state in States)
            {
                if (state.AnimatorInstanceId == _animatorInstatnceId && state.Id == layer.CurrentStateIndex)
                {
                    animationClipId = state.AnimationClipId;
                    break;
                }
            }
            bool animationFound = false;
            AnimationBuffer animationClip = new AnimationBuffer();
            foreach (var animation in Animations)
            {
                if (animation.AnimatorInstanceId == _animatorInstatnceId && animation.Id == animationClipId)
                {
                    animationClip = animation;
                    animationFound = true;
                    break;
                }
            }
            if (!animationFound) // animation clip somehow not found, returning it
            {
                return;
            }

            // animation found let's animate
            // manage timers
            var animationDuration = animationClip.Length;
            var looped = animationClip.Looped;
            var currentTimer = layer.AnimationTime;
            var loopEnded = false;
            if (currentTimer > animationDuration)
            {
                if (looped)
                {
                    currentTimer = currentTimer % animationDuration;
                }
                else
                {
                    currentTimer = animationDuration;
                    loopEnded = true;
                }
            }
            // loop over paths
            foreach (var part in actorParts)
            {
                // positions
                float3 firstPosition = float3.zero;
                float3 secondPosition = float3.zero;
                bool firstPositionFound = false;
                bool secondPositionFound = false;
                float firstPositionTime = -1f;
                float secondPositionTime = -1f;
                foreach (var position in Positions)
                {
                    if (position.AnimationId == animationClipId && part.Path == position.Path)
                    {
                        if (currentTimer <= position.Time)
                        {
                            if (!firstPositionFound)
                            {
                                firstPosition = position.Value;
                                firstPositionTime = position.Time;
                                firstPositionFound = true;
                            }
                            else
                            {
                                if (firstPositionTime > position.Time)
                                {
                                    firstPosition = position.Value;
                                    firstPositionTime = position.Time;
                                }
                            }
                        }
                        else
                        {
                            if (!secondPositionFound)
                            {
                                secondPosition = position.Value;
                                secondPositionTime = position.Time;
                                secondPositionFound = true;
                            }
                            else
                            {
                                if (secondPositionTime < position.Time)
                                {
                                    secondPosition = position.Value;
                                    secondPositionTime = position.Time;
                                }
                            }
                        }
                    }
                }
                // rotations
                quaternion firstRotation = quaternion.identity;
                quaternion secondRotation = quaternion.identity;
                bool firstRotationFound = false;
                bool secondRotationFound = false;
                float firstRotationTime = -1f;
                float secondRotationTime = -1f;
                foreach (var rotation in Rotations)
                {
                    if (rotation.AnimationId == animationClipId && part.Path == rotation.Path)
                    {
                        if (currentTimer <= rotation.Time)
                        {
                            if (!firstRotationFound)
                            {
                                firstRotation = rotation.Value;
                                firstRotationTime = rotation.Time;
                                firstRotationFound = true;
                            }
                            else
                            {
                                if (firstRotationTime > rotation.Time)
                                {
                                    firstRotation = rotation.Value;
                                    firstRotationTime = rotation.Time;
                                }
                            }
                        }
                        else
                        {
                            if (!secondRotationFound)
                            {
                                secondRotation = rotation.Value;
                                secondRotationTime = rotation.Time;
                                secondRotationFound = true;
                            }
                            else
                            {
                                if (secondRotationTime < rotation.Time)
                                {
                                    secondRotation = rotation.Value;
                                    secondRotationTime = rotation.Time;
                                }
                            }
                        }
                    }
                }

                // get current data from local transform
                Entity partEntity = part.Value;
                RefRO<LocalTransform> partLocaltTransform = LocalTransformLookup.GetRefRO(partEntity);
                float3 newPosition = partLocaltTransform.ValueRO.Position;
                quaternion newRotation = partLocaltTransform.ValueRO.Rotation;
                float scale = partLocaltTransform.ValueRO.Scale;

                // calculcate rates
                if (loopEnded)
                {
                    if (firstPositionFound)
                    {
                        newPosition = firstPosition;
                    }
                    if (firstRotationFound)
                    {
                        newRotation = firstRotation;
                    }
                }
                else
                {
                    if (secondPositionFound)
                    {
                        float rate = (currentTimer - firstPositionTime) / (secondPositionTime - firstPositionTime);
                        newPosition = math.lerp(firstPosition, secondPosition, rate);
                    }
                    if (secondRotationFound)
                    {
                        float rate = (currentTimer - firstRotationTime) / (secondRotationTime - firstRotationTime);
                        newRotation = math.slerp(firstRotation, secondRotation, rate);
                    }
                }

                if (part.Path == (FixedString512Bytes)"basic_rig/basic_rig Pelvis/basic_rig L Thigh/basic_rig L Calf")
                {
                    
                }

                // apply
                ParallelWriter.SetComponent(sortKey, partEntity, new LocalTransform
                {
                    Position = newPosition,
                    Rotation = newRotation,
                    Scale = scale
                });
            }
            currentTimer += deltaTime;
            layer.AnimationTime = currentTimer;
        }
    }
}

/*
foreach (var part in actorParts)
{
    // loop each animation key
    bool fisrtPositionFound = false;
    bool secondPositionFound = false;
    bool firstRotationFound = false;
    bool secondRotationFound = false;
    var firstPositionKey = new AnimationKeyBuffer();
    var secondPositionKey = new AnimationKeyBuffer();
    var firstRotationKey = new AnimationKeyBuffer();
    var secondRotationKey = new AnimationKeyBuffer();
    foreach (var animationKey in AnimationKeys)
    {
        if (animationKey.AnimatorInstanceId == _animatorInstatnceId && animationKey.AnimationId == animationClipId && part.Path == animationKey.Path)
        {
            if (animationKey.PositionEngaged) // positions
            {
                if (animationKey.Time <= currentTimer) // first value
                {
                    if (!fisrtPositionFound)
                    {
                        firstPositionKey = animationKey;
                        fisrtPositionFound = true;
                    }
                    else if (!keysAlreadySortedByTime)
                    {
                        if (animationKey.Time > firstPositionKey.Time)
                        {
                            firstPositionKey = animationKey;
                        }
                    }
                }
                else // second value
                {
                    if (!secondPositionFound)
                    {
                        secondPositionKey = animationKey;
                        secondPositionFound = true;
                    }
                    else if (!keysAlreadySortedByTime)
                    {
                        if (animationKey.Time > secondPositionKey.Time)
                        {
                            secondPositionKey = animationKey;
                        }
                    }
                }
            }
            if (animationKey.RotationEngaged) // rotaions
            {
                if (animationKey.Time <= currentTimer) // first value
                {
                    if (!firstRotationFound)
                    {
                        firstRotationKey = animationKey;
                        secondRotationFound = true;
                    }
                    else if (!keysAlreadySortedByTime)
                    {
                        if (animationKey.Time > firstRotationKey.Time)
                        {
                            firstRotationKey = animationKey;
                        }
                    }
                }
                else // second value
                {
                    if (!secondRotationFound)
                    {
                        secondRotationKey = animationKey;
                        secondRotationFound = true;
                    }
                    else if (!keysAlreadySortedByTime)
                    {
                        if (animationKey.Time > secondRotationKey.Time)
                        {
                            secondRotationKey = animationKey;
                        }
                    }
                }
            }
        }
    }
    // calculate new position and rotation
    Entity partEntity = part.Value;
    RefRO<LocalTransform> partLocaltTransform = LocalTransformLookup.GetRefRO(partEntity);
    float3 newPosition = partLocaltTransform.ValueRO.Position;
    quaternion newRotation = partLocaltTransform.ValueRO.Rotation;
    if (loopEnded)
    {
        if (secondPositionFound)
        {
            newPosition = secondPositionKey.PositionValue;
        }
        if (secondRotationFound)
        {
            newRotation = secondRotationKey.RotationValue;
        }
    }
    else
    {
        float positionRate = (currentTimer - firstPositionKey.Time) / (secondPositionKey.Time - firstPositionKey.Time);
        float rotationRate = (currentTimer - firstRotationKey.Time) / (secondRotationKey.Time - firstRotationKey.Time);
        if (secondPositionFound)
        {
            newPosition = math.lerp(firstPositionKey.PositionValue, secondPositionKey.PositionValue, positionRate);
        }
        if (secondRotationFound)
        {
            newRotation = math.slerp(firstRotationKey.RotationValue, secondRotationKey.RotationValue, rotationRate);
        }
    }
    // apply new values
    float scale = partLocaltTransform.ValueRO.Scale;
    ParallelWriter.SetComponent(sortKey, partEntity, new LocalTransform
    {
        Position = newPosition,
        Rotation = newRotation,
        Scale = scale
    });
    // debug
    if (part.Path == (FixedString512Bytes)"basic_rig")
    {
        //Debug.Log(setPosition);
        //Debug.Log($"Time: {currentTimer} FIRST Y: {firstKey.PositionValue.y} SECOND Y: {secondKey.PositionValue.y}");
        //Debug.Log($"firstKey Time: {firstKey.Time} secondKey Time: {secondKey.Time}");
    }
}
*/