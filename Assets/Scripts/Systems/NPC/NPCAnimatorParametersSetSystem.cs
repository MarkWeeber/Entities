using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
[UpdateBefore(typeof(AnimatorAnimateSystem))]
[UpdateAfter(typeof(NPCMovementSystem))]
public partial struct NPCAnimatorParametersSetSystem : ISystem
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
        EntityQuery acrtorsQuery = SystemAPI.QueryBuilder()
            .WithAll<
             AnimatorActorComponent,
             AnimatorActorParametersBuffer,
             MovementStatisticData,
             NPCMovementComponent,
             NPCStrategyBuffer >()
            .Build();

        if (acrtorsQuery.CalculateEntityCount() < 1)
        {
            return;
        }
        state.Dependency = new AnimatorParametersSetJob { }.ScheduleParallel(acrtorsQuery, state.Dependency);
    }

    [BurstCompile]
    private partial struct AnimatorParametersSetJob : IJobEntity
    {
        [BurstCompile]
        private void Execute(
            ref DynamicBuffer<AnimatorActorParametersBuffer> parameters,
            RefRO<MovementStatisticData> movementStatisticData,
            RefRO<NPCMovementComponent> npcMovementComponent,
            in DynamicBuffer<NPCStrategyBuffer> strategyBuffer)
        {
            // acquire current strategy
            var currentStrategyType = NPCStrategyType.NoStrategy;
            foreach (var strategy in strategyBuffer)
            {
                if (strategy.Active)
                {
                    currentStrategyType = strategy.StrategyType;
                    break;
                }
            }
            if (currentStrategyType == NPCStrategyType.NoStrategy)
            {
                return;
            }
            // animator parameters
            var moveSpeedParameterName = (FixedString32Bytes)"MoveSpeed";
            var attackingParameterName = (FixedString32Bytes)"Attacking";
            var dieParameterName = (FixedString32Bytes)"Die";
            bool3 allfound = new bool3();
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.ParameterName == moveSpeedParameterName)
                {
                    allfound.x = true;
                    parameter.NumericValue = movementStatisticData.ValueRO.Speed;
                }
                if (parameter.ParameterName == attackingParameterName)
                {
                    allfound.y = true;
                    parameter.BoolValue = false;
                    if (
                        (currentStrategyType == NPCStrategyType.LookForPlayer || currentStrategyType == NPCStrategyType.FleeForHealth)
                        && movementStatisticData.ValueRO.DestinationReached
                        && npcMovementComponent.ValueRO.TargetVisionState == NPCTargetVisionState.Visible)
                    {
                        parameter.BoolValue = true;
                    }
                }
                parameters[i] = parameter;
                if (allfound.x && allfound.y && allfound.z)
                {
                    break;
                }
            }
        }
    }
}