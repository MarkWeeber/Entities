using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public partial struct AnimationSystem : ISystem
{
    private float timer;
    private bool offsetTimerEngaged;
    private float offsetTimer;
    private double lastUpdateTime;
    private BufferLookup<AnimationPartComponent> animationPartLookup;
    private BufferLookup<KeyFrameComponent> keyFramesLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        timer = 0f;
        offsetTimerEngaged = false;
        offsetTimer = 0f;
        lastUpdateTime = 0d;
        state.RequireForUpdate<AnimationBaseComponent>();
        animationPartLookup = state.GetBufferLookup<AnimationPartComponent>(isReadOnly: true);
        keyFramesLookup = state.GetBufferLookup<KeyFrameComponent>(isReadOnly: true);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        if (SystemAPI.TryGetSingleton<SystemControllerData>(out SystemControllerData systemControllerData))
        {
            if (!systemControllerData.Animation)
            {
                return;
            }
            if (!offsetTimerEngaged && systemControllerData.AnimationOffset > 0f)
            {
                offsetTimer = systemControllerData.AnimationOffset;
                offsetTimerEngaged = true;
            }
            if (offsetTimerEngaged && offsetTimer > 0f)
            {
                offsetTimer -= deltaTime; // offset timer wait
                return;
            }
            if (systemControllerData.AnimationRate > 0f)
            {
                if (timer < 0f)
                {
                    timer = systemControllerData.AnimationRate;
                }
                else
                {
                    timer -= deltaTime; // rate timer wait
                    return;
                }
            }
        }
        else
        {
            return;
        }
        float actualDeltaTime = (float) (SystemAPI.Time.ElapsedTime - lastUpdateTime);
        lastUpdateTime = SystemAPI.Time.ElapsedTime;
        DynamicBuffer<AnimationBaseComponent> animationBaseBuffer = SystemAPI.GetSingletonBuffer<AnimationBaseComponent>();
        animationPartLookup.Update(ref state);
        keyFramesLookup.Update(ref state);
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
        AnimationsJob animationsJob = new AnimationsJob
        {
            ParallelWriter = parallelWriter,
            AnimationBasesDynamicBuffer = animationBaseBuffer,
            AnimationPartLookup = animationPartLookup,
            KeyFramesLookup = keyFramesLookup,
            DeltaTime = actualDeltaTime
        };
        JobHandle jobHandle = animationsJob.ScheduleParallel(state.Dependency);
        jobHandle.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    [WithAll(typeof(AnimationActorComponent))]
    [WithAll(typeof(AnimationPartComponent))]
    private partial struct AnimationsJob : IJobEntity
    {
        internal EntityCommandBuffer.ParallelWriter ParallelWriter;
        [ReadOnly]
        public DynamicBuffer<AnimationBaseComponent> AnimationBasesDynamicBuffer;
        [ReadOnly]
        public BufferLookup<AnimationPartComponent> AnimationPartLookup;
        [ReadOnly]
        public BufferLookup<KeyFrameComponent> KeyFramesLookup;
        public float DeltaTime;
        [BurstCompile]
        private void Execute
            (
                [ChunkIndexInQuery] int sortKey,
                Entity entiy,
                RefRW<AnimationActorComponent> animationActorComponent
            )
        {
            if (AnimationPartLookup.TryGetBuffer(entiy, out DynamicBuffer<AnimationPartComponent> animationPartsDynamicBuffer))
            {
                FixedString32Bytes animationName = animationActorComponent.ValueRO.AnimationName;
                float currentAnimationTime = animationActorComponent.ValueRO.AnimationTime;
                bool actorNonLoopAnimationReached = animationActorComponent.ValueRO.NonLoopAnimationReached;
                KeyFrameComponent start = new KeyFrameComponent();
                KeyFrameComponent end = new KeyFrameComponent();
                for (int i = 0; i < AnimationBasesDynamicBuffer.Length; i++)
                {
                    if (AnimationBasesDynamicBuffer[i].AnimationName == animationName) // animation found
                    {
                        float animationDuration = AnimationBasesDynamicBuffer[i].AnimationDuration;
                        bool nonLoopedAnimationEnded = false;
                        bool isAnimationLooped = AnimationBasesDynamicBuffer[i].Loop;
                        if (actorNonLoopAnimationReached && !isAnimationLooped) // animation already completed no need to animate
                        {
                            return;
                        }
                        else if (actorNonLoopAnimationReached && isAnimationLooped) // clearing loop blocker if animation was changer or reset
                        {
                            animationActorComponent.ValueRW.NonLoopAnimationReached = false;
                        }
                        if (currentAnimationTime > animationDuration) // clamping animation timer
                        {
                            if (!isAnimationLooped)
                            {
                                nonLoopedAnimationEnded = true;
                                currentAnimationTime = animationDuration;
                                animationActorComponent.ValueRW.NonLoopAnimationReached = true;
                            }
                            else
                            {
                                currentAnimationTime = currentAnimationTime % animationDuration;
                            }
                            //animationActorComponent.ValueRW.AnimationTime = currentAnimationTime;
                        }
                        if (KeyFramesLookup.TryGetBuffer(AnimationBasesDynamicBuffer[i].AnimationHolder, out DynamicBuffer<KeyFrameComponent> keyFrames))
                        {
                            for (int k = 0; k < animationPartsDynamicBuffer.Length; k++) // cycle through parts
                            {
                                Entity partEntity = animationPartsDynamicBuffer[k].Entity; // part entity
                                bool firstFound = false;
                                bool secondFound = false;
                                for (int j = 0; j < keyFrames.Length; j++)
                                {
                                    if (animationPartsDynamicBuffer[k].Name == keyFrames[j].Name) // part found
                                    {
                                        if (currentAnimationTime >= keyFrames[j].Time)
                                        {
                                            firstFound = true;
                                            start = new KeyFrameComponent
                                            {
                                                Time = keyFrames[j].Time,
                                                Position = keyFrames[j].Position,
                                                Rotation = keyFrames[j].Rotation,
                                            };
                                        }
                                        if (currentAnimationTime <= keyFrames[j].Time)
                                        {
                                            secondFound = true;
                                            end = new KeyFrameComponent
                                            {
                                                Time = keyFrames[j].Time,
                                                Position = keyFrames[j].Position,
                                                Rotation = keyFrames[j].Rotation,
                                            };
                                        }
                                    }
                                    if (firstFound && secondFound) // both keyframes found
                                    {
                                        float3 newPosition = new float3();
                                        quaternion newRotation = new quaternion();
                                        if (!nonLoopedAnimationEnded) // looping animation
                                        {
                                            // calculate animation complete percentage
                                            float delta = currentAnimationTime - start.Time;
                                            float range = end.Time - start.Time;
                                            float animationPercentage = delta / range;
                                            // positioning
                                            newPosition = math.lerp(start.Position, end.Position, animationPercentage);
                                            // rotation
                                            newRotation = math.slerp(start.Rotation, end.Rotation, animationPercentage);
                                            // apply transforms
                                        }
                                        else // non loop end animation - needs to be done just once per animation
                                        {
                                            newPosition = end.Position;
                                            newRotation = end.Rotation;
                                        }
                                        ParallelWriter.SetComponent<LocalTransform>(sortKey, partEntity, new LocalTransform
                                        {
                                            Position = newPosition,
                                            Rotation = newRotation,
                                            Scale = 1f
                                        });
                                        break; // animating the specific part completed
                                    }
                                }
                            }
                        }
                        // shift animation timer
                        currentAnimationTime += DeltaTime;
                        animationActorComponent.ValueRW.AnimationTime = currentAnimationTime;
                        break; // animating completed
                    }
                }
            }
        }
    }
}