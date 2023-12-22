using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct ProjectileRaycastedAbilitiesSystem : ISystem
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
        // raycasted bouncy projectiles
        EntityQuery bouncyProjectilesQuery = SystemAPI.QueryBuilder().WithAll<RayCastData, LocalTransform, BouncyProjectileTag>().Build();
        new BounceOnRayCastHitJob { }.ScheduleParallel(bouncyProjectilesQuery);

        // raycasted self-descruct projectiles
        EntityQuery selfDestructProjectilesQuery = SystemAPI.QueryBuilder().WithAll<RayCastData, DestructibleProjectileTag>().Build();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
        DestructOnRayCastHitJob destructOnRayCastHitJob = new DestructOnRayCastHitJob
        {
            ParallelWriter = parallelWriter,
        };
        JobHandle destructingRayCastsHandle = destructOnRayCastHitJob.ScheduleParallel(selfDestructProjectilesQuery, state.Dependency);
        destructingRayCastsHandle.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    [BurstCompile]
    private partial struct DestructOnRayCastHitJob : IJobEntity
    {
        internal EntityCommandBuffer.ParallelWriter ParallelWriter;
        [BurstCompile]
        private void Execute
            (
                [ChunkIndexInQuery] int sortKey,
                Entity entity,
                RefRO<RayCastData> rayCastData
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
        [BurstCompile]
        private void Execute
            (
                RefRO<RayCastData> rayCastData,
                RefRW<LocalTransform> localTransform
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