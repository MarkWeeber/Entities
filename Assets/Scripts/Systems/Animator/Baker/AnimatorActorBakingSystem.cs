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
                AnimatorActorPartBufferComponent>()
            .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
            .Build();

        if (animatorActorEntities.CalculateEntityCount() < 1)
        {
            return;
        }
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();

        state.Dependency = new BindPartsWithRootEntity
        {
            ParallelWriter = parallelWriter,
        }.ScheduleParallel(animatorActorEntities, state.Dependency);

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    [BurstCompile]
    private partial struct BindPartsWithRootEntity : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ParallelWriter;
        [BurstCompile]
        private void Execute(
            [ChunkIndexInQuery] int sortKey,
            DynamicBuffer<AnimatorActorPartBufferComponent> parts,
            Entity entity)
        {
            // adding part component and buffers for each of parts
            foreach (var part in parts)
            {
                Entity partEntity = part.Value;
                ParallelWriter.AddComponent(sortKey, partEntity, new AnimatorPartComponent{ RootEntity = entity });
            }
        }
    }
}