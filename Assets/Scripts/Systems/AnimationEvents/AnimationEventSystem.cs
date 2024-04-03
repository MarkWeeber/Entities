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
        private void Execute(ref DynamicBuffer<AnimationEventBuffer> animationEventBuffer, Entity entity)
        {
            for (int i = animationEventBuffer.Length - 1; i >= 0; i--)
            {
                var animationEvent = animationEventBuffer[i];
                if (animationEvent.EventType == AnimationEventType.Attack)
                {
                    CastDamage(animationEvent);
                }
                if (animationEvent.EventType == AnimationEventType.HealUp)
                {
                    HealUp(animationEvent, entity);
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
                        if (HealthLookup.IsComponentEnabled(hit.Entity))
                        {
                            var healthData = HealthLookup.GetRefRW(hit.Entity);
                            var newHealth = healthData.ValueRO.CurrentHealth - animationEvent.EventValue;
                            healthData.ValueRW.CurrentHealth = math.clamp(newHealth, 0f, healthData.ValueRO.MaxHealth);
                            // if (newHealth < 0f)
                            // {
                            //     HealthLookup.SetComponentEnabled(hit.Entity, false);
                            // }
                        }
                    }
                }
            }
            hits.Dispose();
        }

        [BurstCompile]
        private void HealUp(AnimationEventBuffer animationEvent, Entity entity)
        {
            var collisionFilter = new CollisionFilter
            {
                BelongsTo = uint.MaxValue,
                CollidesWith = animationEvent.EventCollisionTags.Value
            };
            if (CollisionWorld.CheckSphere(animationEvent.EventPosition, animationEvent.EventRadius, collisionFilter))
            {
                if (HealthLookup.HasComponent(entity))
                {
                    var healthComponet = HealthLookup.GetRefRW(entity);
                    var newHealth = healthComponet.ValueRO.CurrentHealth + animationEvent.EventValue;
                    healthComponet.ValueRW.CurrentHealth = math.clamp(newHealth, 0f, healthComponet.ValueRO.MaxHealth);
                }
            }
        }
    }
}