using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
public partial struct NPCSetMovementSystem : ISystem
{
    private ComponentLookup<ColliderCollisionData> collisionDataLookup;
    private ComponentLookup<PhysicsCollider> physicsColliderLookup;
    private ComponentLookup<NPCMovementComponent> npcMovementLookup;
    private ComponentLookup<LocalToWorld> localToWorldLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        collisionDataLookup = state.GetComponentLookup<ColliderCollisionData>(false);
        physicsColliderLookup = state.GetComponentLookup<PhysicsCollider>(true);
        npcMovementLookup = state.GetComponentLookup<NPCMovementComponent>(false);
        localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
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
        CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        collisionDataLookup.Update(ref state);
        physicsColliderLookup.Update(ref state);
        npcMovementLookup.Update(ref state);
        localToWorldLookup.Update(ref state);
        state.Dependency = new VisionTirggerJob
        {
            CollisionDataLookup = collisionDataLookup,
            CollisionWorld = collisionWorld,
            LocalToWorldLookup = localToWorldLookup,
            NPCMovementLookup = npcMovementLookup,
            PhysicsColliderLookup = physicsColliderLookup
        }.Schedule(simulation, state.Dependency);
    }

    [BurstCompile]
    private partial struct VisionTirggerJob : ITriggerEventsJob
    {
        public ComponentLookup<ColliderCollisionData> CollisionDataLookup;
        [ReadOnly]
        public ComponentLookup<PhysicsCollider> PhysicsColliderLookup;
        public ComponentLookup<NPCMovementComponent> NPCMovementLookup;
        [ReadOnly]
        public CollisionWorld CollisionWorld;
        [ReadOnly]
        public ComponentLookup<LocalToWorld> LocalToWorldLookup;
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
                // check filtering of target entity
                var colliderData = CollisionDataLookup.GetRefRW(colliderDataEntity);
                var colliderBlobAsset = PhysicsColliderLookup.GetRefRO(colliderTargetEntity).ValueRO.Value;
                ref var targetCollider = ref colliderBlobAsset.Value;
                var belongsTo = targetCollider.GetCollisionFilter().BelongsTo;
                var collidestWith = colliderData.ValueRO.CollisionFilter.CollidesWith;
                if ((belongsTo & collidestWith) > 0) // if target belongs to collision filter
                {
                    colliderData.ValueRW.IsColliding = true;
                    var originPosition = LocalToWorldLookup.GetRefRO(colliderDataEntity);
                    var targetPosition = LocalToWorldLookup.GetRefRO(colliderTargetEntity);
                    // raycast
                    var raycastInput = new RaycastInput()
                    {
                        Start = originPosition.ValueRO.Position,
                        End = targetPosition.ValueRO.Position,
                        Filter = colliderData.ValueRO.CollisionFilter
                    };
                    var rayCastHit = new Unity.Physics.RaycastHit();
                    if (CollisionWorld.CastRay(raycastInput, out rayCastHit))
                    {
                        if (rayCastHit.Entity == colliderTargetEntity) // target is visible, no obstacles in a way
                        {
                            var npcMovementData = NPCMovementLookup.GetRefRW(colliderData.ValueRO.ParentEntity);
                            npcMovementData.ValueRW.IsDestinationSet = true;
                            npcMovementData.ValueRW.Destination = targetPosition.ValueRO.Position;
                        }
                    }
                }
            }
        }
    }
}