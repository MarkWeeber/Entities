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
        EntityQuery colliders = SystemAPI.QueryBuilder()
            .WithAll<
                NPCStrategyBuffer,
                NPCVisionSettings,
                NPCMovementComponent,
                LocalToWorld,
                RandomComponent
            >().Build();
        int entityCount = colliders.CalculateEntityCount();
        if (entityCount < 1)
        {
            return;
        }
        CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        state.Dependency = new VisionCastJob
        {
            CollisionWorld = collisionWorld
        }.ScheduleParallel(colliders, state.Dependency);
    }

    [BurstCompile]
    private partial struct VisionCastJob : IJobEntity
    {
        [ReadOnly]
        public CollisionWorld CollisionWorld;
        [BurstCompile]
        private void Execute(
            in DynamicBuffer<NPCStrategyBuffer> strategyBuffer,
            RefRO<NPCVisionSettings> npcVisionData,
            RefRO<LocalToWorld> localToWorld,
            RefRW<NPCMovementComponent> npcMovementComponent,
            RefRW<RandomComponent> randomComponent,
            Entity entity)
        {
            var randomVector = new float3(randomComponent.ValueRW.Random.NextFloat(-1f, 1f), 0f, randomComponent.ValueRW.Random.NextFloat(-1f, 1f));
            randomVector = math.normalize(randomVector);
            var randomRange1 = randomComponent.ValueRW.Random.NextFloat(0f, 1f);
            var randomRange2 = randomComponent.ValueRW.Random.NextFloat(0f, 1f);
            bool targetIsVisible = false;
            var targetDestination = float3.zero;
            var currentStrategy = new NPCStrategyBuffer();
            var exludeLayerTags = npcVisionData.ValueRO.DisregardTags.Value;
            var targetLayerTags = uint.MaxValue ^ exludeLayerTags;
            foreach (var strategy in strategyBuffer)
            {
                if (strategy.Active)
                {
                    currentStrategy = strategy;
                    break;
                }
            }
            if (!currentStrategy.Active)
            {
                return;
            }
            var collisionFilter = new CollisionFilter { BelongsTo = uint.MaxValue, CollidesWith = currentStrategy.TargetCollider.Value };
            if (CheckTargetVisibility(
                collisionFilter,
                localToWorld,
                npcVisionData,
                ref targetDestination,
                targetLayerTags,
                entity)) // player visible
            {
                npcMovementComponent.ValueRW.TargetVisionState = NPCTargetVisionState.Visible;
                targetIsVisible = true;
            }
            else // player not visible, wander more
            {
                if (npcMovementComponent.ValueRO.WaitTimer <= 0f && npcMovementComponent.ValueRO.DestinationReached)
                {
                    if (SetRandomDestination(
                        localToWorld,
                        npcVisionData,
                        ref targetDestination,
                        randomVector,
                        randomRange2,
                        currentStrategy,
                        targetLayerTags))
                    {
                        npcMovementComponent.ValueRW.TargetVisionState = NPCTargetVisionState.Lost;
                        targetIsVisible = true;
                    }
                }
            }
            if (targetIsVisible)
            {
                npcMovementComponent.ValueRW.Destination = targetDestination;
                npcMovementComponent.ValueRW.WaitTimer
                    = currentStrategy.MinWaitTime + (math.abs(currentStrategy.MinWaitTime - currentStrategy.MaxWaitTime) * randomRange1);
            }
        }

        [BurstCompile]
        private bool CheckTargetVisibility(
            CollisionFilter collisionFilter,
            RefRO<LocalToWorld> localToWorld,
            RefRO<NPCVisionSettings> npcVisionData,
            ref float3 targetDestination,
            uint targetLayerTags,
            Entity entity)
        {
            bool result = false;
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
                    var targetPosition = float3.zero;
                    targetPosition = hit.Position;
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
                            CollidesWith = targetLayerTags
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
                                if (rayCastHits[i].Entity == entity || rayCastHits[i].Entity == hit.Entity)
                                {
                                    targetDestination = targetPosition;
                                    result = true;
                                }
                                else
                                {
                                    targetDestination = float3.zero;
                                    result = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            targetDestination = targetPosition;
                            result = true;
                        }
                        rayCastHits.Dispose();
                    }
                }
            }
            hits.Dispose();
            return result;
        }

        [BurstCompile]
        private bool CheckIfNoObstaclesInWay(float3 origin, float3 target, uint targetLayerTags)
        {
            bool result = false;
            var collisionFilter = new CollisionFilter
            {
                BelongsTo = uint.MaxValue,
                CollidesWith = targetLayerTags
            };
            var raycastInput = new RaycastInput()
            {
                Start = origin,
                End = target,
                Filter = collisionFilter
            };
            var rayCastHits = new NativeList<Unity.Physics.RaycastHit>(Allocator.Temp);
            if (!CollisionWorld.CastRay(raycastInput, ref rayCastHits)) // check if no obstacles in vision way
            {
                result = true;
            }
            return result;
        }

        [BurstCompile]
        private bool SetRandomDestination(
            RefRO<LocalToWorld> localToWorld,
            RefRO<NPCVisionSettings> npcVisionData,
            ref float3 target,
            float3 randomVector,
            float randomRange2,
            NPCStrategyBuffer currentStrategy,
            uint targetLayerTags)
        {
            bool result = false;
            var origin = localToWorld.ValueRO.Position + npcVisionData.ValueRO.VisionOffset;
            var multiplier = currentStrategy.MinWanderRange + math.abs(currentStrategy.MaxWanderRange - currentStrategy.MinWanderRange) * randomRange2;
            multiplier = math.clamp(multiplier, 0.01f, multiplier);
            target = origin + randomVector * multiplier;
            result = CheckIfNoObstaclesInWay(origin, target, targetLayerTags);
            return result;
        }
    }
}