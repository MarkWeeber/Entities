using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics.Systems;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
public partial struct SimpleCollisionSystem : ISystem
{
    private ComponentLookup<ColliderCollisionData> collisionDataLookup;
    private ComponentLookup<PhysicsCollider> physicsColliderLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        collisionDataLookup = state.GetComponentLookup<ColliderCollisionData>(false);
        physicsColliderLookup = state.GetComponentLookup<PhysicsCollider>(true);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityQuery colliders = SystemAPI.QueryBuilder().WithAll<ColliderCollisionData, PhysicsCollider, LocalTransform>().Build();
        if (colliders.CalculateEntityCount() < 1)
        {
            return;
        }
        SimulationSingleton simulation = SystemAPI.GetSingleton<SimulationSingleton>();
        collisionDataLookup.Update(ref state);
        physicsColliderLookup.Update(ref state);
        state.Dependency = new VisionTirggerJob
        {
            CollisionDataLookup = collisionDataLookup,
            PhysicsColliderLookup = physicsColliderLookup
        }.Schedule(simulation, state.Dependency);
    }

    [BurstCompile]
    private partial struct VisionTirggerJob : ITriggerEventsJob
    {
        public ComponentLookup<ColliderCollisionData> CollisionDataLookup;
        [ReadOnly]
        public ComponentLookup<PhysicsCollider> PhysicsColliderLookup;
        [BurstCompile]
        public void Execute(TriggerEvent triggerEvent)
        {
            Entity colliderDataEntity = Entity.Null;
            Entity colliderTargetEntity = Entity.Null;
            if (CollisionDataLookup.HasComponent(triggerEvent.EntityA))
            {
                colliderDataEntity = triggerEvent.EntityA;
                colliderTargetEntity = triggerEvent.EntityB;
            }
            if (CollisionDataLookup.HasComponent(triggerEvent.EntityB))
            {
                colliderDataEntity = triggerEvent.EntityB;
                colliderTargetEntity = triggerEvent.EntityA;
            }
            if (colliderDataEntity != Entity.Null && colliderTargetEntity != Entity.Null)
            {
                var colliderData = CollisionDataLookup.GetRefRW(colliderDataEntity);
                var colliderBlobAsset = PhysicsColliderLookup.GetRefRO(colliderTargetEntity).ValueRO.Value;
                ref var targetCollider = ref colliderBlobAsset.Value;
                var belongsTo = targetCollider.GetCollisionFilter().BelongsTo;
                var collidestWith = colliderData.ValueRO.CollisionFilter.CollidesWith;
                if ((belongsTo & collidestWith) > 0)
                {
                    colliderData.ValueRW.IsColliding = true;
                }
            }
        }
    }
}