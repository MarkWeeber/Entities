using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
[UpdateAfter(typeof(AnimatorAnimateSystem))]
public partial struct AnimationEventSystem : ISystem
{
    private ComponentLookup<HealthData> healthLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        healthLookup = state.GetComponentLookup<HealthData>(false);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var entitiesWithEvents = SystemAPI.QueryBuilder().WithAll<AnimationEventBuffer>().Build();
        if (entitiesWithEvents.CalculateEntityCount() < 1)
        {
            return;
        }
        healthLookup.Update(ref state);
        CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        state.Dependency = new PerformEventsJob
        {
            CollisionWorld = collisionWorld,
            HealthLookup = healthLookup,
        }.Schedule(entitiesWithEvents, state.Dependency);
    }

    [BurstCompile]
    private partial struct PerformEventsJob : IJobEntity
    {
        [ReadOnly]
        public CollisionWorld CollisionWorld;
        public ComponentLookup<HealthData> HealthLookup;
        [BurstCompile]
        private void Execute(ref DynamicBuffer<AnimationEventBuffer> animationEventBuffer)
        {
            for (int i = animationEventBuffer.Length - 1; i >= 0; i--)
            {
                var animationEvent = animationEventBuffer[i];
                if (animationEvent.EventType == AnimationEventType.Attack)
                {
                    CastDamage(animationEvent);
                }
                animationEventBuffer.RemoveAt(i);
            }
        }

        [BurstCompile]
        private void CastDamage(AnimationEventBuffer animationEvent)
        {
            var collisionFilter = new CollisionFilter
            {
                BelongsTo = uint.MaxValue,
                CollidesWith = animationEvent.EventCollisionTags.Value
            };
            var hits = new NativeList<DistanceHit>(Allocator.Temp);
            if (CollisionWorld.OverlapSphere(animationEvent.EventPosition, animationEvent.EventRadius, ref hits, collisionFilter))
            {
                foreach (var hit in hits)
                {
                    if (HealthLookup.HasComponent(hit.Entity))
                    {
                        var healthData = HealthLookup.GetRefRW(hit.Entity);
                        healthData.ValueRW.CurrentHealth = 
                            math.clamp(healthData.ValueRO.CurrentHealth - animationEvent.EventValue, 0f, healthData.ValueRO.MaxHealth);
                    }
                }
            }
            hits.Dispose();
        }
    }
}