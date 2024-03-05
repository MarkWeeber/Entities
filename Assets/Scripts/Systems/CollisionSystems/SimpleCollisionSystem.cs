using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct SimpleCollisionSystem : ISystem
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
        EntityQuery colliders = SystemAPI.QueryBuilder().WithAll<ColliderCollisionData, PhysicsCollider, LocalTransform>().Build();
        if (colliders.CalculateEntityCount() < 1)
        {
            return;
        }
        CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        state.Dependency = new CastCollisionJob
        { }.ScheduleParallel(colliders, state.Dependency);
    }

    [BurstCompile]
    private partial struct CastCollisionJob : IJobEntity
    {
        [BurstCompile]
        private void Execute(RefRW<ColliderCollisionData> data, RefRO<LocalTransform> localTransform, PhysicsCollider physicsCollider, Entity entity)
        {
            ref var colliderAsset = ref physicsCollider.Value;
            ref var collider = ref physicsCollider.Value.Value;
            collider.SetCollisionFilter(data.ValueRO.CollisionFilter);
            //var pointer = (Collider*) colliderAsset.GetUnsafePtr();
            var colliderCastInput = new ColliderCastInput(colliderAsset, localTransform.ValueRO.Position, localTransform.ValueRO.Position, localTransform.ValueRO.Rotation);
            var hits = new NativeList<ColliderCastHit>();
            var hasHit = collider.CastCollider(colliderCastInput, ref hits);
            if (hasHit)
            {
                data.ValueRW.IsColliding = true;
                data.ValueRW.CollisionNumber = hits.Length;
                Debug.Log("CHECK");
            }
            hits.Dispose();
        }
    }
}