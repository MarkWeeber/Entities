using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;

[BurstCompile]
public partial struct AnimationSystem : ISystem
{
    private float timer;
    private double lastUpdateTime;
    private BufferLookup<AnimationPartComponent> animationPartLookup;
    private BufferLookup<KeyFrameComponent> keyFramesLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        timer = 0f;
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
        if (SystemAPI.TryGetSingletonRW<SystemControllerData>(out RefRW<SystemControllerData> systemControllerData))
        {
            if (!systemControllerData.ValueRO.Animation)
            {
                return;
            }
            if (systemControllerData.ValueRO.AnimationOffset > 0f)
            {
                systemControllerData.ValueRW.AnimationOffset -= deltaTime;
                return;
            }
            if (systemControllerData.ValueRO.AnimationRate > 0f)
            {
                if (timer < 0f)
                {
                    timer = systemControllerData.ValueRO.AnimationRate;
                }
                else
                {
                    timer -= deltaTime;
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
                KeyFrameComponent start = new KeyFrameComponent();
                KeyFrameComponent end = new KeyFrameComponent();
                for (int i = 0; i < AnimationBasesDynamicBuffer.Length; i++)
                {
                    if (AnimationBasesDynamicBuffer[i].AnimationName == animationName)
                    {
                        float animationDuration = AnimationBasesDynamicBuffer[i].AnimationDuration;
                        if (currentAnimationTime > animationDuration)
                        {
                            currentAnimationTime = currentAnimationTime % animationDuration;
                            animationActorComponent.ValueRW.AnimationTime = currentAnimationTime;
                        }
                        if (KeyFramesLookup.TryGetBuffer(AnimationBasesDynamicBuffer[i].AnimationHolder, out DynamicBuffer<KeyFrameComponent> keyFrames))
                        {
                            for (int k = 0; k < animationPartsDynamicBuffer.Length; k++)
                            {
                                Entity partEntity = animationPartsDynamicBuffer[k].Entity;
                                bool firstFound = false;
                                bool secondFound = false;
                                for (int j = 0; j < keyFrames.Length; j++)
                                {
                                    if (animationPartsDynamicBuffer[k].Name == keyFrames[j].Name)
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
                                    if (firstFound && secondFound)
                                    {
                                        // calculate animation complete percentage
                                        float delta = currentAnimationTime - start.Time;
                                        float range = end.Time - start.Time;
                                        float animationPercentage = delta / range;
                                        // positioning
                                        float3 newPosition = math.lerp(start.Position, end.Position, animationPercentage);
                                        // rotation
                                        quaternion newRotation = math.slerp(start.Rotation, end.Rotation, animationPercentage);
                                        // apply transforms
                                        ParallelWriter.SetComponent<LocalTransform>(sortKey, partEntity, new LocalTransform
                                        {
                                            Position = newPosition,
                                            Rotation = newRotation,
                                            Scale = 1f
                                        });
                                        break;
                                    }
                                }
                                //if (!firstFound || !secondFound)
                                //{
                                //    // nothing found or reached end of animation
                                //    animationActorComponent.ValueRW.AnimationTime = 0f;
                                //}
                            }
                        }
                    }
                }
                // shift animation timer
                currentAnimationTime += DeltaTime;
                animationActorComponent.ValueRW.AnimationTime = currentAnimationTime;
            }
        }
    }
}