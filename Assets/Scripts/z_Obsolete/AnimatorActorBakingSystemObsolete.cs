using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;

//[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateBefore(typeof(SpawnerSystem))]
public partial struct AnimatorActorBakingSystemObsolete : ISystem
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
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
        EntityQuery animatorActorEntities = SystemAPI.QueryBuilder()
            .WithAll<
                AnimatorActorComponent,
                AnimatorActorPartBufferComponent,
                AnimatorActorBakedComponent,
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
            DynamicBuffer<AnimationRotationBuffer> rotations,
            Entity entity)
        {
            NativeArray<AnimationPositionBuffer> _position = positions.AsNativeArray();
            _position.Sort(new CompareAnimationPositionTimeBuffer());
            NativeArray<AnimationRotationBuffer> _rotations = rotations.AsNativeArray();
            _rotations.Sort(new CompareAnimationRotationTimeBuffer());
            // adding part component and buffers for each of parts
            foreach (var part in parts)
            {
                Entity partEntity = part.Value;
                // part component
                ParallelWriter.AddComponent(sortKey, partEntity, new AnimatorActorPartComponent());
                // positions
                ParallelWriter.AddBuffer<AnimationPartPositionBuffer>(sortKey, partEntity);
                for (int i = 0; i < _position.Length; i++)
                {
                    var item = _position[i];
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
                for (int i = 0; i < _rotations.Length; i++)
                {
                    var item = _rotations[i];
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
            _position.Dispose();
            _rotations.Dispose();
            ParallelWriter.SetComponentEnabled<AnimatorActorBakedComponent>(sortKey, entity, false);
        }
    }
}