using Unity.Collections;
using Unity.Deformations;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateBefore(typeof(DeformationsInPresentation))]
public partial struct MaterialOverrideSystem : ISystem
{
    private ComponentLookup<MovementStatisticData> movementLookup;
    private ComponentLookup<HealthData> healthLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        movementLookup = state.GetComponentLookup<MovementStatisticData>(true);
        healthLookup = state.GetComponentLookup<HealthData>(true);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //state.Enabled = false;
        var entityQuery = SystemAPI.QueryBuilder()
            .WithAll<MaterialSineSpeedOverride, MaterialHealthRatioOverride, LinkedEntityComponent>().Build();
        movementLookup.Update(ref state);
        healthLookup.Update(ref state);
        float deltaTime = SystemAPI.Time.DeltaTime;
        state.Dependency = new NPCMaterialOverrideJob
        {
            MovementLookup = movementLookup,
            HealthLookup = healthLookup
        }.ScheduleParallel(entityQuery, state.Dependency);
    }

    [BurstCompile]
    private partial struct NPCMaterialOverrideJob : IJobEntity
    {
        [ReadOnly]
        public ComponentLookup<MovementStatisticData> MovementLookup;
        [ReadOnly]
        public ComponentLookup<HealthData> HealthLookup;
        [BurstCompile]
        private void Execute(
            RefRW<MaterialSineSpeedOverride> sineSpeedOverride,
            RefRW<MaterialHealthRatioOverride> healthOverride,
            RefRO<LinkedEntityComponent> linkedEntity)
        {
            if (MovementLookup.HasComponent(linkedEntity.ValueRO.Value))
            {
                var speed = MovementLookup.GetRefRO(linkedEntity.ValueRO.Value).ValueRO.Speed;
                sineSpeedOverride.ValueRW.Value = speed;
            }
            if (HealthLookup.HasComponent(linkedEntity.ValueRO.Value))
            {
                var healthRatio = 
                    HealthLookup.GetRefRO(linkedEntity.ValueRO.Value).ValueRO.CurrentHealth /
                    HealthLookup.GetRefRO(linkedEntity.ValueRO.Value).ValueRO.MaxHealth;
                healthOverride.ValueRW.Value = healthRatio;
            }
        }
    }
}