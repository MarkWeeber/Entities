using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
            NativeArray<AnimationKeyBuffer> animationKeys = SystemAPI.GetSingletonBuffer<AnimationKeyBuffer>().AsNativeArray();
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
                AnimationKeys = animationKeys,
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
            animationKeys.Dispose();
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
        public NativeArray<AnimationKeyBuffer> AnimationKeys;
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
                AnimateLayer(sortKey, ref layer, ref actorParts, DeltaTime, false);
                actorLayers[layerIndex] = layer;
            }
        }

        [BurstCompile]
        private void AnimateLayer(
            int sortKey,
            ref AnimatorActorLayerBuffer layer,
            ref DynamicBuffer<AnimatorActorPartBufferComponent> actorParts,
            float deltaTime,
            bool keysAlreadySortedByTime)
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
            //looped = false;
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
                // loop each animation key
                bool firstFound = false;
                bool secondFound = false;
                var firstKey = new AnimationKeyBuffer();
                var secondKey = new AnimationKeyBuffer();
                foreach (var animationKey in AnimationKeys)
                {
                    if (animationKey.AnimatorInstanceId == _animatorInstatnceId && animationKey.AnimationId == animationClipId && part.Path == animationKey.Path)
                    {
                        if (animationKey.Time <= currentTimer)
                        {
                            if (!firstFound)
                            {
                                firstKey = animationKey;
                                firstFound = true;
                            }
                            else if (!keysAlreadySortedByTime)
                            {
                                if (animationKey.Time > firstKey.Time)
                                {
                                    firstKey = animationKey;
                                }
                            }
                        }
                        else
                        {
                            if (!secondFound)
                            {
                                secondKey = animationKey;
                                secondFound = true;
                            }
                            else if (!keysAlreadySortedByTime)
                            {
                                if (animationKey.Time < secondKey.Time)
                                {
                                    secondKey = animationKey;
                                }
                            }
                        }
                    }
                    if (keysAlreadySortedByTime)
                    {
                        if ((firstFound && secondFound) || (firstFound && loopEnded))
                        {
                            break;
                        }
                    }
                }
                // calculate new position and rotation
                float3 newPosition = float3.zero;
                quaternion newRotation = quaternion.identity;
                if (loopEnded)
                {
                    if (firstKey.PositionEngaged)
                    {
                        newPosition = firstKey.PositionValue;
                    }
                    if (firstKey.RotationEulerEngaged)
                    {
                        newRotation = quaternion.Euler(firstKey.RotationEulerValue.z, firstKey.RotationEulerValue.y, firstKey.RotationEulerValue.z);
                    }
                    if (firstKey.RotationEngaged)
                    {
                        newRotation = new quaternion(firstKey.RotationValue);
                    }
                }
                else
                {
                    float rate = (currentTimer - firstKey.Time) / (secondKey.Time - firstKey.Time);
                    if (firstKey.PositionEngaged)
                    {
                        newPosition = math.lerp(firstKey.PositionValue, secondKey.PositionValue, rate);
                    }
                    if (firstKey.RotationEulerEngaged)
                    {
                        newRotation = math.slerp(
                            quaternion.Euler(
                                math.radians(firstKey.RotationEulerValue.x),
                                math.radians(firstKey.RotationEulerValue.y),
                                math.radians(firstKey.RotationEulerValue.z)),
                            quaternion.Euler(
                                math.radians(secondKey.RotationEulerValue.x),
                                math.radians(secondKey.RotationEulerValue.y),
                                math.radians(secondKey.RotationEulerValue.z)),
                            rate);
                    }
                    if (firstKey.RotationEngaged)
                    {
                        newRotation = math.slerp(new quaternion(firstKey.RotationValue), new quaternion(secondKey.RotationValue), rate);
                    }
                }
                // apply new values
                Entity partEntity = part.Value;
                RefRO<LocalTransform> partLocaltTransform = LocalTransformLookup.GetRefRO(partEntity);
                float3 setPosition = partLocaltTransform.ValueRO.Position;
                quaternion setRotation = partLocaltTransform.ValueRO.Rotation;
                float scale = partLocaltTransform.ValueRO.Scale;
                if (firstKey.PositionEngaged)
                {
                    setPosition = newPosition;
                }
                if (firstKey.RotationEngaged || firstKey.RotationEulerEngaged)
                {
                    setRotation = newRotation;
                }
                ParallelWriter.SetComponent(sortKey, partEntity, new LocalTransform
                {
                    Position = setPosition,
                    Rotation = setRotation,
                    Scale = scale
                });
            }
            currentTimer += deltaTime;
            layer.AnimationTime = currentTimer;
        }
    }
}