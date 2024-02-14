using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[BurstCompile]
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
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
        EntityQuery animatorActorEntities = SystemAPI.QueryBuilder()
            .WithAll<
                AnimatorActorComponent,
                AnimatorActorPartBufferComponent,
                AnimationPositionBuffer,
                AnimationRotationBuffer>()
            .Build();

        if (animatorActorEntities.CalculateEntityCount() < 1)
        {
            ecb.Dispose();
            return;
        }

        state.Dependency = new PrepareAnimatorActorJob
        {
            ParallelWriter = parallelWriter,
        }.ScheduleParallel(animatorActorEntities, state.Dependency);

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    [BurstCompile]
    private partial struct PrepareAnimatorActorJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ParallelWriter;
        [BurstCompile]
        private void Execute(
            [ChunkIndexInQuery] int sortKey,
            DynamicBuffer<AnimatorActorPartBufferComponent> parts,
            DynamicBuffer<AnimationPositionBuffer> positions,
            DynamicBuffer<AnimationRotationBuffer> rotations)
        {
            // adding part component and buffers for each of parts
            foreach (var part in parts)
            {
                Entity partEntity = part.Value;
                // part component
                ParallelWriter.AddComponent(sortKey, partEntity, new AnimatorActorPartComponent());
                // positions
                ParallelWriter.AddBuffer<AnimationPartPositionBuffer>(sortKey, partEntity);
                for (int i = 0; i < positions.Length; i++)
                {
                    var item = positions[i];
                    if (item.Path == part.Path)
                    {
                        var partItem = new AnimationPartPositionBuffer
                        {
                            AnimationId = item.AnimationId,
                            Time = item.Time,
                            Value = item.Value,
                        };
                        ParallelWriter.AppendToBuffer(sortKey, partEntity, partItem);
                    }
                }
                // rotations
                ParallelWriter.AddBuffer<AnimationPartRotationBuffer>(sortKey, partEntity);
                for (int i = 0; i < rotations.Length; i++)
                {
                    var item = rotations[i];
                    if (item.Path == part.Path)
                    {
                        var partItem = new AnimationPartRotationBuffer
                        {
                            AnimationId = item.AnimationId,
                            Time = item.Time,
                            Value = item.Value,
                        };
                        ParallelWriter.AppendToBuffer(sortKey, partEntity, partItem);
                    }
                }
            }
        }
    }
}