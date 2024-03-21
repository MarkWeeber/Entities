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
    private NativeList<Unity.Mathematics.Random> randoms;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
        randoms = new NativeList<Unity.Mathematics.Random>(Allocator.Persistent);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        randoms.Dispose();
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
        //float2 randomVector = float2.zero;
        //float randomRange = 0f;
        //if (SystemAPI.TryGetSingletonRW<RandomComponent>(out RefRW<RandomComponent> randomComponent))
        //{
        //    randomVector = new float2
        //    {
        //        x = randomComponent.ValueRW.Random.NextFloat(-1f, 1f),
        //        y = randomComponent.ValueRW.Random.NextFloat(-1f, 1f),
        //    };
        //    randomRange = randomComponent.ValueRW.Random.NextFloat(0f, 1f);
        //    randomVector = math.normalize(randomVector);
        //}
        int entityCount = colliders.CalculateEntityCount();
        if (entityCount < 1)
        {
            return;
        }
        int randomDifference = entityCount - randoms.Length;
        if (randomDifference > 0)
        {
            for (int i = randoms.Length + 1; i < entityCount + 1; i++)
            {
                randoms.Add(new Unity.Mathematics.Random((uint)i));
            }
        }
        else if (randomDifference < 0)
        {
            for (int i = randoms.Length - 1; i > entityCount - 1; i--)
            {
                randoms.RemoveAt(i);
            }
        }
        CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        localToWorldLookup.Update(ref state);
        state.Dependency = new VisionCastJob
        {
            Randoms = randoms,
            EntityCount = entityCount,
            CollisionWorld = collisionWorld,
            LocalToWorldLookup = localToWorldLookup
        }.ScheduleParallel(colliders, state.Dependency);
    }

    [BurstCompile]
    private partial struct VisionCastJob : IJobEntity
    {
        [Unity.Collections.LowLevel.Unsafe.NativeDisableContainerSafetyRestriction]
        public NativeList<Unity.Mathematics.Random> Randoms;
        public int EntityCount;
        [ReadOnly]
        public CollisionWorld CollisionWorld;
        [ReadOnly]
        public ComponentLookup<LocalToWorld> LocalToWorldLookup;
        [BurstCompile]
        private void Execute(
            [EntityIndexInQuery] int entityIndexInQuery,
            in DynamicBuffer<NPCStrategyBuffer> strategyBuffer,
            RefRO<NPCVisionSettings> npcVisionData,
            RefRO<LocalToWorld> localToWorld,
            RefRW<NPCMovementComponent> npcMovementComponent,
            Entity entity)
        {
            var currentRandom = Randoms[entityIndexInQuery];
            var randomVector = new float3(Randoms[entityIndexInQuery].NextFloat(-1f, 1f), 0f, Randoms[entityIndexInQuery].NextFloat(-1f, 1f));
            randomVector = math.normalize(randomVector);
            var randomRange = Randoms[entityIndexInQuery].NextFloat(0f, 1f);
            bool targetIsVisible = false;
            var targetDestination = float3.zero;
            var currentStrategy = new NPCStrategyBuffer();
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
                entity))
            {
                targetIsVisible = true;
            }
            else
            {
                if (npcMovementComponent.ValueRO.WaitTimer <= 0f)
                {
                    if (SetRandomDestination(localToWorld, npcVisionData, ref targetDestination, randomVector, npcMovementComponent.ValueRO.WanderDistance, entity))
                    {
                        targetIsVisible = true;
                    }
                }
            }
            if (targetIsVisible)
            {
                npcMovementComponent.ValueRW.TargetVisionState = NPCTargetVisionState.Visible;
                npcMovementComponent.ValueRW.Destination = targetDestination;
                npcMovementComponent.ValueRW.WaitTimer
                    = currentStrategy.MinWaitTime + (math.distance(currentStrategy.MinWaitTime, currentStrategy.MaxWaitTime) * randomRange);
            }
        }

        [BurstCompile]
        private bool CheckTargetVisibility(
            CollisionFilter collisionFilter,
            RefRO<LocalToWorld> localToWorld,
            RefRO<NPCVisionSettings> npcVisionData,
            ref float3 targetDestination,
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
                                    result = true;
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
            hits.Dispose();
            return result;
        }

        [BurstCompile]
        private bool CheckIfNoObstaclesInWay(Entity entity, float3 origin, float3 target)
        {
            bool result = false;
            var collisionFilter = new CollisionFilter
            {
                BelongsTo = uint.MaxValue,
                CollidesWith = uint.MaxValue
            };
            var raycastInput = new RaycastInput()
            {
                Start = origin,
                End = target,
                Filter = collisionFilter
            };
            var rayCastHits = new NativeList<Unity.Physics.RaycastHit>(Allocator.Temp);
            if (CollisionWorld.CastRay(raycastInput, ref rayCastHits)) // check if no obstacles in vision way
            {
                if (rayCastHits.Length == 1 && rayCastHits[0].Entity == entity)
                {
                    result = true;
                }
            }
            return result;
        }

        [BurstCompile]
        private bool SetRandomDestination(
            RefRO<LocalToWorld> localToWorld,
            RefRO<NPCVisionSettings> npcVisionData,
            ref float3 target,
            float3 randomVector,
            float minDistance,
            Entity entity)
        {
            bool result = false;
            var origin = localToWorld.ValueRO.Position + npcVisionData.ValueRO.VisionOffset;
            target = origin + randomVector * minDistance;
            result = CheckIfNoObstaclesInWay(entity, origin, target);
            return result;
        }
    }
}