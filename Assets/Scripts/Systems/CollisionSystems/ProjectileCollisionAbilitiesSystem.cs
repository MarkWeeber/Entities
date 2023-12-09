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
    private ComponentLookup<BouncyProjectileTag> bouncyLookup;
    private ComponentLookup<DestructibleProjectileTag> destructibleLookup;
    private ComponentLookup<LocalTransform> localTransformLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        bouncyLookup = state.GetComponentLookup<BouncyProjectileTag>(false);
        destructibleLookup = state.GetComponentLookup<DestructibleProjectileTag>(false);
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
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        bouncyLookup.Update(ref state);
        destructibleLookup.Update(ref state);
        localTransformLookup.Update(ref state);
        ProjectileCollisionJob projectileCollisionJob = new ProjectileCollisionJob
        {
            EntityCommandBuffer = entityCommandBuffer,
            BouncyLookup = bouncyLookup,
            DestructibleLookup = destructibleLookup,
            LocalTransformLookup = localTransformLookup
        };
        JobHandle jobHandle = projectileCollisionJob.Schedule(simulation, state.Dependency);
        jobHandle.Complete();
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
    [BurstCompile]
    private partial struct ProjectileCollisionJob : ICollisionEventsJob
    {
        public EntityCommandBuffer EntityCommandBuffer;
        public ComponentLookup<BouncyProjectileTag> BouncyLookup;
        public ComponentLookup<DestructibleProjectileTag> DestructibleLookup;
        public ComponentLookup<LocalTransform> LocalTransformLookup;

        public void Execute(CollisionEvent collisionEvent)
        {
            if (BouncyLookup.HasComponent(collisionEvent.EntityA))
            {
                ReflectEntity(collisionEvent.EntityA, collisionEvent.Normal);
            }
            if (BouncyLookup.HasComponent(collisionEvent.EntityB))
            {
                ReflectEntity(collisionEvent.EntityB, collisionEvent.Normal);
            }
            if (DestructibleLookup.HasComponent(collisionEvent.EntityA))
            {
                EntityCommandBuffer.DestroyEntity(collisionEvent.EntityA);
            }
            if (DestructibleLookup.HasComponent(collisionEvent.EntityB))
            {
                EntityCommandBuffer.DestroyEntity(collisionEvent.EntityB);
            }
        }

        private void ReflectEntity(Entity entity, float3 normal)
        {
            RefRW<LocalTransform> localTransform = LocalTransformLookup.GetRefRW(entity);
            float3 reflectionDirection = math.reflect(localTransform.ValueRO.Forward(), normal);
            quaternion reflectedRotation = quaternion.LookRotation(reflectionDirection, math.up());
            localTransform.ValueRW.Rotation = reflectedRotation;
        }
    }
}