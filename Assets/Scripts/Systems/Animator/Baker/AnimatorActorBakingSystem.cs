using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[BurstCompile]
//[UpdateInGroup(typeof(InitializationSystemGroup))]
//[UpdateBefore(typeof(SpawnerSystem))]
public partial struct AnimatorActorBakingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        EntityQuery animatorActorEntities = SystemAPI.QueryBuilder()
            .WithAll<
                AnimatorActorComponent,
                AnimatorActorLayerBuffer,
                AnimatorActorPartBufferComponent>()
            .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
            .Build();

        if (animatorActorEntities.CalculateEntityCount() < 1)
        {
            return;
        }
        if (SystemAPI.TryGetSingletonBuffer<AnimationBlobBuffer>(out DynamicBuffer<AnimationBlobBuffer> animations))
        {
            var animationArray = animations.AsNativeArray();
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();

            state.Dependency = new BindPartsWithRootEntity
            {
                Animations = animationArray,
                ParallelWriter = parallelWriter,
            }.ScheduleParallel(animatorActorEntities, state.Dependency);

            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            animationArray.Dispose();
        }

    }
    [BurstCompile]
    private partial struct BindPartsWithRootEntity : IJobEntity
    {
        public NativeArray<AnimationBlobBuffer> Animations;
        public EntityCommandBuffer.ParallelWriter ParallelWriter;
        [BurstCompile]
        private void Execute(
            [ChunkIndexInQuery] int sortKey,
            RefRO<AnimatorActorComponent> animatorActorComponent,
            DynamicBuffer<AnimatorActorPartBufferComponent> parts,
            DynamicBuffer<AnimatorActorLayerBuffer> layers,
            Entity entity)
        {
            // adding part component and buffers for each of parts
            foreach (var part in parts)
            {
                Entity partEntity = part.Value;
                foreach (var layer in layers)
                {
                    int animatorInstanceId = animatorActorComponent.ValueRO.AnimatorId;
                    var relevantAnimation = new AnimationBlobBuffer();
                    bool relevantAnimationFound = false;
                    foreach (var animation in Animations)
                    {
                        if (animation.AnimatorInstanceId == animatorInstanceId)
                        {
                            relevantAnimation = animation;
                            relevantAnimationFound = true;
                            break;
                        }
                    }
                    if (relevantAnimationFound)
                    {
                        ref PathDataPool pathDataPool = ref relevantAnimation.PathData.Value;
                        ref var pathsPool = ref pathDataPool.PathData;
                        for (int i = 0; i < pathsPool.Length; i++)
                        {
                            ref var pathData = ref pathsPool[i];
                            if (pathData.Path == part.Path)
                            {
                                ParallelWriter.AddComponent(sortKey, partEntity, new AnimatorPartComponent
                                {
                                    RootEntity = entity,
                                    PathAnimationBlobIndex = i
                                });
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}