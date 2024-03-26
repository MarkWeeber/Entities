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
    private ComponentLookup<EnemyTag> enemyLookup;
    private ComponentLookup<HealthData> healthLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        enemyLookup = state.GetComponentLookup<EnemyTag>(true);
        healthLookup = state.GetComponentLookup<HealthData>(false);
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

        // raycasted damage dealing projectiles
        var damagingProjectiles = SystemAPI.QueryBuilder().WithAll<RayCastData, ProjectileComponent>().Build();
        enemyLookup.Update(ref state);
        healthLookup.Update(ref state);
        var projectileDealDamageJobHandle = new ProjectileDealDamageJob
        {
            EnemyLookup = enemyLookup,
            HealthLookup = healthLookup
        }.Schedule(damagingProjectiles, state.Dependency);

        // raycasted self-descruct projectiles
        EntityQuery selfDestructProjectilesQuery = SystemAPI.QueryBuilder().WithAll<RayCastData, DestructibleProjectileTag>().Build();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        var destructOnRayCastHitJobHandle = new DestructOnRayCastHitJob
        {
            ECB = ecb,
        }.Schedule(selfDestructProjectilesQuery, projectileDealDamageJobHandle);
        destructOnRayCastHitJobHandle.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    [BurstCompile]
    private partial struct DestructOnRayCastHitJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        [BurstCompile]
        private void Execute
            (
                Entity entity,
                RefRO<RayCastData> rayCastData
            )
        {
            if (rayCastData.ValueRO.RaycastHit.Entity != Entity.Null)
            {
                ECB.DestroyEntity(entity);
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

    [BurstCompile]
    private partial struct ProjectileDealDamageJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<EnemyTag> EnemyLookup;
        public ComponentLookup<HealthData> HealthLookup;
        [BurstCompile]
        private void Execute(RefRO<RayCastData> raycastData, RefRO<ProjectileComponent> projectileComponent)
        {
            var hitEntity = raycastData.ValueRO.RaycastHit.Entity;
            if (EnemyLookup.HasComponent(hitEntity))
            {
                if (HealthLookup.HasComponent(hitEntity))
                {
                    var healthData = HealthLookup.GetRefRW(hitEntity);
                    healthData.ValueRW.CurrentHealth =
                        math.clamp(healthData.ValueRO.CurrentHealth - projectileComponent.ValueRO.Damage, 0f, healthData.ValueRO.MaxHealth);
                }
            }
        }
    }
}