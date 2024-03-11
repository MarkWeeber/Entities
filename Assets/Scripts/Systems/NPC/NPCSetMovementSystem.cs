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
        EntityQuery colliders = SystemAPI.QueryBuilder().WithAll<NPCVisionData, NPCMovementComponent, LocalToWorld>().Build();
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
        private void Execute(RefRW<NPCVisionData> npcVisionData, RefRO<LocalToWorld> localToWorld, RefRW<NPCMovementComponent> npcMovementComponent, Entity entity)
        {
            bool targetIsVisilbe = false;
            var originOffset = npcVisionData.ValueRO.VisionOffset;
            var originStart = localToWorld.ValueRO.Position + originOffset;
            var radius = npcVisionData.ValueRO.SpherCastRadius;
            var collisionFilter = npcVisionData.ValueRO.CollisionFilter;
            var hits = new NativeList<DistanceHit>(Allocator.Temp);
            if (CollisionWorld.OverlapSphere(originStart, radius, ref hits, collisionFilter))
            {
                var forward = localToWorld.ValueRO.Forward;
                var upward = localToWorld.ValueRO.Up;
                var right = localToWorld.ValueRO.Right;
                foreach (var hit in hits)
                {
                    var targetPosition = LocalToWorldLookup.GetRefRO(hit.Entity).ValueRO.Position + originOffset;
                    //Debug.DrawRay(originStart, targetPosition, Color.red);
                    var hitDirection = targetPosition - originStart;
                    //Debug.DrawRay(originStart, hitDirection, Color.blue);
                    var xDirection = new float3(0f, hitDirection.y, 0f);
                    var yDirection = new float3(hitDirection.x, 0f, hitDirection.z);
                    var zDirection = new float3(hitDirection.x, hitDirection.y, 0f);
                    //Debug.DrawRay(originStart, xDirection, Color.red);
                    //Debug.DrawRay(originStart, zDirection, Color.blue);
                    //Debug.DrawRay(originStart, yDirection, Color.green);
                    var xAngle = math.abs(Vector3.SignedAngle(forward, forward + xDirection, right));
                    var yAngle = math.abs(Vector3.SignedAngle(forward, yDirection, upward));
                    var zAngle = math.abs(Vector3.SignedAngle(forward, zDirection, right));
                    npcVisionData.ValueRW.Data.x = xAngle;
                    npcVisionData.ValueRW.Data.y = yAngle;
                    npcVisionData.ValueRW.Data.z = zAngle;
                    npcVisionData.ValueRW.Data = targetPosition;
                    if (yAngle <= npcVisionData.ValueRO.FOV.x && xAngle <= npcVisionData.ValueRO.FOV.y) // object is withing fov
                    {
                        // check if no obstacles in vision way
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
                        if (CollisionWorld.CastRay(raycastInput, ref rayCastHits))
                        {
                            for (int i = 0; i < rayCastHits.Length; i++)
                            {
                                if (rayCastHits[i].Entity == hit.Entity)
                                {
                                    Debug.DrawLine(originStart, targetPosition, Color.red);
                                    break;
                                    npcMovementComponent.ValueRW.IsDestinationSet = true;
                                    npcMovementComponent.ValueRW.Destination = targetPosition;
                                    targetIsVisilbe = true;
                                    Debug.Log("CHECK");
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
            npcVisionData.ValueRW.IsColliding = targetIsVisilbe;
            hits.Dispose();
        }
    }
}