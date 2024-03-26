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
[UpdateBefore(typeof(PhysicsSystemGroup))]
public partial struct ProjectileCollisionAbilitiesSystem : ISystem
{
    private ComponentLookup<EnemyTag> enemyLookup;
    private ComponentLookup<HealthData> healthLookup;
    private ComponentLookup<ProjectileComponent> projectileLookup;
    private ComponentLookup<BouncyProjectileTag> bouncyLookup;
    private ComponentLookup<DestructibleProjectileTag> destructibleLookup;
    private ComponentLookup<LocalTransform> localTransformLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        projectileLookup = state.GetComponentLookup<ProjectileComponent>(true);
        enemyLookup = state.GetComponentLookup<EnemyTag>(true);
        healthLookup = state.GetComponentLookup<HealthData>(false);
        bouncyLookup = state.GetComponentLookup<BouncyProjectileTag>(true);
        destructibleLookup = state.GetComponentLookup<DestructibleProjectileTag>(true);
        localTransformLookup = state.GetComponentLookup<LocalTransform>(false);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        SimulationSingleton simulation = SystemAPI.GetSingleton<SimulationSingleton>();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        bouncyLookup.Update(ref state);
        destructibleLookup.Update(ref state);
        localTransformLookup.Update(ref state);
        projectileLookup.Update(ref state);
        enemyLookup.Update(ref state);
        healthLookup.Update(ref state);
        ProjectileCollisionJob projectileCollisionJob = new ProjectileCollisionJob
        {
            EntityCommandBuffer = ecb,
            ProjectileLookup = projectileLookup,
            HealthLookup = healthLookup,
            EnemyLookup = enemyLookup,
            BouncyLookup = bouncyLookup,
            DestructibleLookup = destructibleLookup,
            LocalTransformLookup = localTransformLookup
        };
        JobHandle jobHandle = projectileCollisionJob.Schedule(simulation, state.Dependency);
        jobHandle.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    [BurstCompile]
    private partial struct ProjectileCollisionJob : ICollisionEventsJob
    {
        public EntityCommandBuffer EntityCommandBuffer;
        [ReadOnly] public ComponentLookup<ProjectileComponent> ProjectileLookup;
        [ReadOnly] public ComponentLookup<EnemyTag> EnemyLookup;
        public ComponentLookup<HealthData> HealthLookup;
        [ReadOnly] public ComponentLookup<BouncyProjectileTag> BouncyLookup;
        [ReadOnly] public ComponentLookup<DestructibleProjectileTag> DestructibleLookup;
        public ComponentLookup<LocalTransform> LocalTransformLookup;
        [BurstCompile]
        public void Execute(CollisionEvent collisionEvent)
        {
            ManageHitOnEnemyCollisionEvent(collisionEvent);
            ManageReflectionProjectiles(collisionEvent);
            ManageSelfDestructProjectiles(collisionEvent);
        }

        [BurstCompile]
        private void ManageHitOnEnemyCollisionEvent(CollisionEvent collisionEvent)
        {
            if (EnemyLookup.HasComponent(collisionEvent.EntityA) && ProjectileLookup.HasComponent(collisionEvent.EntityB))
            {
                Debug.Log("AA");
                if (HealthLookup.HasComponent(collisionEvent.EntityA))
                {
                    var damageValue = ProjectileLookup.GetRefRO(collisionEvent.EntityB).ValueRO.Damage;
                    HealthLookup.GetRefRW(collisionEvent.EntityA).ValueRW.CurrentHealth -= damageValue;
                    Debug.Log("A");
                }
            }
            if (EnemyLookup.HasComponent(collisionEvent.EntityB) && ProjectileLookup.HasComponent(collisionEvent.EntityA))
            {
                Debug.Log("BB");
                if (HealthLookup.HasComponent(collisionEvent.EntityB))
                {
                    var damageValue = ProjectileLookup.GetRefRO(collisionEvent.EntityA).ValueRO.Damage;
                    HealthLookup.GetRefRW(collisionEvent.EntityB).ValueRW.CurrentHealth -= damageValue;
                    Debug.Log("B");
                }
            }
        }

        [BurstCompile]
        private void ManageReflectionProjectiles(CollisionEvent collisionEvent)
        {
            if (BouncyLookup.HasComponent(collisionEvent.EntityA))
            {
                ReflectEntity(collisionEvent.EntityA, collisionEvent.Normal);
            }
            if (BouncyLookup.HasComponent(collisionEvent.EntityB))
            {
                ReflectEntity(collisionEvent.EntityB, collisionEvent.Normal);
            }
        }

        [BurstCompile]
        private void ManageSelfDestructProjectiles(CollisionEvent collisionEvent)
        {
            if (DestructibleLookup.HasComponent(collisionEvent.EntityA))
            {
                EntityCommandBuffer.DestroyEntity(collisionEvent.EntityA);
            }
            if (DestructibleLookup.HasComponent(collisionEvent.EntityB))
            {
                EntityCommandBuffer.DestroyEntity(collisionEvent.EntityB);
            }
        }

        [BurstCompile]
        private void ReflectEntity(Entity entity, float3 normal)
        {
            RefRW<LocalTransform> localTransform = LocalTransformLookup.GetRefRW(entity);
            float3 reflectionDirection = math.reflect(localTransform.ValueRO.Forward(), normal);
            quaternion reflectedRotation = quaternion.LookRotation(reflectionDirection, math.up());
            localTransform.ValueRW.Rotation = reflectedRotation;
        }
    }
}