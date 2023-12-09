using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[UpdateBefore(typeof(ExportPhysicsWorld))]
[UpdateAfter(typeof(RayCastHitUpdateSystem))]
public partial struct ProjectileAbilitiesSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RayCastData>();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // self-descruct ray casters
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
        DestructOnRayCastHitJob destructOnRayCastHitJob = new DestructOnRayCastHitJob
        {
            ParallelWriter = parallelWriter,
        };
        JobHandle destructingRayCastsHandle = destructOnRayCastHitJob.ScheduleParallel(state.Dependency);
        destructingRayCastsHandle.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        // bouncy raycasters
        BounceOnRayCastHitJob bounceOnRayCastHitJob = new BounceOnRayCastHitJob { };
        JobHandle bounceRayCastsHandle = bounceOnRayCastHitJob.ScheduleParallel(state.Dependency);
        bounceRayCastsHandle.Complete();

    }
    [BurstCompile]
    private partial struct DestructOnRayCastHitJob : IJobEntity
    {
        internal EntityCommandBuffer.ParallelWriter ParallelWriter;
        private void Execute
            (
                [ChunkIndexInQuery] int sortKey,
                Entity entity,
                RefRO<RayCastData> rayCastData,
                DestructOnRayCastHitTag destructOnRayCastHitTag
            )
        {
            if (rayCastData.ValueRO.RaycastHit.Entity != Entity.Null)
            {
                ParallelWriter.DestroyEntity(sortKey, entity);
            }
        }
    }

    [BurstCompile]
    private partial struct BounceOnRayCastHitJob : IJobEntity
    {
        private void Execute
            (
                RefRO<RayCastData> rayCastData,
                RefRW<LocalTransform> localTransform,
                BounceOnRayCastHitTag bounceOnRayCastHitTag
            )
        {
            if (rayCastData.ValueRO.RaycastHit.Entity != Entity.Null)
            {
                float3 reflectionDirection = math.reflect(localTransform.ValueRO.Forward(), rayCastData.ValueRO.RaycastHit.SurfaceNormal);
                quaternion reflectedRotation = quaternion.LookRotation(reflectionDirection, math.up());
                localTransform.ValueRW.Rotation = reflectedRotation;
            }
        }
    }
}