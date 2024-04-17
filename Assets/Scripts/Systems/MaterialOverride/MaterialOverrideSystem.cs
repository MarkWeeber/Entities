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
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        movementLookup = state.GetComponentLookup<MovementStatisticData>(true);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var entityQuery = SystemAPI.QueryBuilder().WithAll<DeformationsSineSpeedOverride>().Build();
        movementLookup.Update(ref state);
        state.Dependency = new NPCMaterialSetJob { MovementLookup = movementLookup }.ScheduleParallel(entityQuery, state.Dependency);
    }

    [BurstCompile]
    private partial struct NPCMaterialSetJob : IJobEntity
    {
        [ReadOnly]
        public ComponentLookup<MovementStatisticData> MovementLookup;
        [BurstCompile]
        private void Execute(RefRW<DeformationsSineSpeedOverride> deformationsSineSpeedOverride)
        {
            if (MovementLookup.HasComponent(deformationsSineSpeedOverride.ValueRO.ParentEntity))
            {
                var speed = MovementLookup.GetRefRO(deformationsSineSpeedOverride.ValueRO.ParentEntity).ValueRO.Speed;
                deformationsSineSpeedOverride.ValueRW.Value = speed * 20f + 1f;
            }
        }
    }
}