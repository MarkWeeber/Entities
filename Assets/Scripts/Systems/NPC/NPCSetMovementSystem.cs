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
    private ComponentLookup<LocalToWorld> localToWorldLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityQuery colliders = SystemAPI.QueryBuilder()
            .WithAll<
            NPCStrategyBuffer,
            NPCVisionSettings,
            NPCMovementComponent,
            LocalToWorld
            >().Build();
        if (colliders.CalculateEntityCount() < 1)
        {
            return;
        }
        CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        localToWorldLookup.Update(ref state);
        state.Dependency = new VisionCastJob
        {
            CollisionWorld = collisionWorld,
            LocalToWorldLookup = localToWorldLookup
        }.ScheduleParallel(colliders, state.Dependency);
    }

    [BurstCompile]
    private partial struct VisionCastJob : IJobEntity
    {
        [ReadOnly]
        public CollisionWorld CollisionWorld;
        [ReadOnly]
        public ComponentLookup<LocalToWorld> LocalToWorldLookup;
        [BurstCompile]
        private void Execute(
            in DynamicBuffer<NPCStrategyBuffer> strategyBuffer,
            RefRO<NPCVisionSettings> npcVisionData,
            RefRO<LocalToWorld> localToWorld,
            RefRW<NPCMovementComponent> npcMovementComponent,
            Entity entity)
        {
            var collisionFilter = new CollisionFilter { BelongsTo = uint.MaxValue, CollidesWith = 0 };
            foreach (var strategy in strategyBuffer)
            {
                if (strategy.Active)
                {
                    collisionFilter.CollidesWith = strategy.TargetCollider.Value;
                    break;
                }
            }
            if (collisionFilter.CollidesWith == 0)
            {
                return;
            }
            bool targetIsVisilbe = false;
            var targetDestination = float3.zero;
            var originOffset = npcVisionData.ValueRO.VisionOffset;
            var originStart = localToWorld.ValueRO.Position + originOffset;
            var radius = npcVisionData.ValueRO.SpherCastRadius;
            var hits = new NativeList<DistanceHit>(Allocator.Temp);
            if (CollisionWorld.OverlapSphere(originStart, radius, ref hits, collisionFilter))
            {
                var forward = localToWorld.ValueRO.Forward;
                var upward = localToWorld.ValueRO.Up;
                var right = localToWorld.ValueRO.Right;
                foreach (var hit in hits)
                {
                    var targetPosition = LocalToWorldLookup.GetRefRO(hit.Entity).ValueRO.Position + originOffset;
                    var hitDirection = targetPosition - originStart;
                    var xDirection = new float3(0f, hitDirection.y, 0f);
                    var yDirection = new float3(hitDirection.x, 0f, hitDirection.z);
                    var xAngle = math.abs(Vector3.SignedAngle(forward, forward + xDirection, right));
                    var yAngle = math.abs(Vector3.SignedAngle(forward, yDirection, upward));
                    if (yAngle <= npcVisionData.ValueRO.FOV.x && xAngle <= npcVisionData.ValueRO.FOV.y) // object is withing fov
                    {
                        var _collisionFilter = new CollisionFilter
                        {
                            BelongsTo = uint.MaxValue,
                            CollidesWith = uint.MaxValue
                        };
                        var raycastInput = new RaycastInput()
                        {
                            Start = originStart,
                            End = targetPosition,
                            Filter = _collisionFilter
                        };
                        var rayCastHits = new NativeList<Unity.Physics.RaycastHit>(Allocator.Temp);
                        if (CollisionWorld.CastRay(raycastInput, ref rayCastHits)) // check if no obstacles in vision way
                        {
                            for (int i = 0; i < rayCastHits.Length; i++)
                            {
                                if (rayCastHits[i].Entity == hit.Entity)
                                {
                                    targetDestination = targetPosition;
                                    targetIsVisilbe = true;
                                    break;
                                }
                                if (rayCastHits[i].Entity != hit.Entity && rayCastHits[i].Entity != entity)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (targetIsVisilbe)
            {
                npcMovementComponent.ValueRW.TargetVisionState = NPCTargetVisionState.Visible;
                npcMovementComponent.ValueRW.Destination = targetDestination;
            }
            else
            {
                if (npcMovementComponent.ValueRO.TargetVisionState == NPCTargetVisionState.Visible)
                {
                    npcMovementComponent.ValueRW.TargetVisionState = NPCTargetVisionState.Lost;
                }
            }
            hits.Dispose();
        }
    }
}