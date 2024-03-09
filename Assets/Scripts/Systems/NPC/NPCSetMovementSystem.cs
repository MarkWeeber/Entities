using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using TMPro;

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
        private void Execute(RefRW<NPCVisionData> npcVisionData, RefRO<LocalToWorld> localToWorld, RefRW<NPCMovementComponent> npcMovementComponent)
        {
            bool targetIsVisilbe = false;
            var originStart = localToWorld.ValueRO.Position + npcVisionData.ValueRO.VisionOffset;
            var radius = npcVisionData.ValueRO.SpherCastRadius;
            var collisionFilter = npcVisionData.ValueRO.CollisionFilter;
            var hits = new NativeList<DistanceHit>(Allocator.Temp);
            if (CollisionWorld.OverlapSphere(originStart, radius, ref hits, collisionFilter))
            {
                var forward = localToWorld.ValueRO.Forward;
                var upward = localToWorld.ValueRO.Up;
                foreach (var hit in hits)
                {
                    var targetPosition = LocalToWorldLookup.GetRefRO(hit.Entity).ValueRO.Position + npcVisionData.ValueRO.VisionOffset;
                    var hitDirection = targetPosition - originStart;
                    var xDirection = new float3(0f, hitDirection.y, hitDirection.z);
                    var yDirection = new float3(hitDirection.x, 0f, hitDirection.z);
                    var zDirection = new float3(hitDirection.x, hitDirection.y, 0f);
                    //Debug.DrawRay(originStart, xDirection, Color.red);
                    //Debug.DrawRay(originStart, zDirection, Color.blue);
                    //Debug.DrawRay(originStart, yDirection, Color.green);
                    var xAngle = math.abs(90f - Vector3.SignedAngle(upward, xDirection, math.up()));
                    var yAngle = math.abs(Vector3.SignedAngle(forward, yDirection, math.up()));
                    var zAngle = math.abs(Vector3.SignedAngle(forward, zDirection, new float3(0f, 0f, 1f)));
                    npcVisionData.ValueRW.Data.x = xAngle;
                    npcVisionData.ValueRW.Data.y = yAngle;
                    npcVisionData.ValueRW.Data.z = zAngle;
                    if (yAngle <= npcVisionData.ValueRO.FOV.x && xAngle <= npcVisionData.ValueRO.FOV.y) // object is withing fov
                    {
                        // check if no obstacles in vision way
                        var raycastInput = new RaycastInput()
                        {
                            Start = originStart,
                            End = targetPosition,
                            Filter = collisionFilter
                        };
                        var rayCastHit = new Unity.Physics.RaycastHit();
                        if (CollisionWorld.CastRay(raycastInput, out rayCastHit))
                        {
                            if (rayCastHit.Entity == hit.Entity)
                            {
                                npcMovementComponent.ValueRW.IsDestinationSet = true;
                                npcMovementComponent.ValueRW.Destination = targetPosition;
                                Debug.DrawRay(originStart, targetPosition, Color.red);
                                targetIsVisilbe = true;
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