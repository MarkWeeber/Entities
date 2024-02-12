using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[BurstCompile]
public partial struct AnimatorActorPartBakingSystemObsolete : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.Enabled = false;
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        EntityCommandBuffer.ParallelWriter parallelWriter = entityCommandBuffer.AsParallelWriter();

        EntityQuery animatorActorRootEntities = SystemAPI.QueryBuilder()
            .WithAll<AnimatorActorPartBufferComponent>().Build();

        state.Dependency = new PrepareAnimatorActorPartsJob
        {
            ParallelWriter = parallelWriter
        }.ScheduleParallel(animatorActorRootEntities, state.Dependency);
        state.Dependency.Complete();
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    [BurstCompile]
    private partial struct PrepareAnimatorActorPartsJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ParallelWriter;
        [BurstCompile]
        private void Execute([ChunkIndexInQuery] int sortKey, in DynamicBuffer<AnimatorActorPartBufferComponent> buffer, Entity entity)
        {
            foreach (var item in buffer)
            {
                var part = new AnimatorActorPartComponent
                {
                    Path = item.Path,
                    RootEntity = entity
                };
                ParallelWriter.AddComponent(sortKey, item.Value, part);
            }
        }
    }
}