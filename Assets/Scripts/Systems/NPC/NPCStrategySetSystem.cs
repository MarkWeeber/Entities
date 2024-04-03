using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(NPCSetMovementSystem))]
public partial struct NPCStrategySetSystem : ISystem
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
        EntityQuery npcsWithStrategies = SystemAPI.QueryBuilder().WithAll<NPCStrategyBuffer, HealthData, MovementData>().Build();
        if (npcsWithStrategies.CalculateEntityCount() < 1)
        {
            return;
        }
        var setNPCStrategiesValuesJobHandle = new SetNPCStrategiesValuesJob { }.ScheduleParallel(npcsWithStrategies, state.Dependency);
        state.Dependency = new UpdateActiveStrategyJob { }.ScheduleParallel(npcsWithStrategies, setNPCStrategiesValuesJobHandle);
    }

    [BurstCompile]
    private partial struct SetNPCStrategiesValuesJob : IJobEntity
    {
        [BurstCompile]
        private void Execute(ref DynamicBuffer<NPCStrategyBuffer> strategyBuffer, RefRO<HealthData> healthData)
        {
            for (int i = 0; i < strategyBuffer.Length; i++)
            {
                var strategy = strategyBuffer[i];
                float healthToMaxRatio = healthData.ValueRO.CurrentHealth / healthData.ValueRO.MaxHealth;
                switch (strategy.StrategyType)
                {
                    case NPCStrategyType.LookForPlayer:
                        strategy.StrategyValue = healthToMaxRatio;
                        break;
                    case NPCStrategyType.FleeForHealth:
                        strategy.StrategyValue = 0.5f / healthToMaxRatio;
                        break;
                    default:
                        break;
                }
                strategyBuffer[i] = strategy;
            }
        }
    }

    [BurstCompile]
    private partial struct UpdateActiveStrategyJob : IJobEntity
    {
        [BurstCompile]
        private void Execute(ref DynamicBuffer<NPCStrategyBuffer> strategyBuffer, RefRW<MovementData> movementData)
        {
            float maxStrategyValue = -1f;
            int indexAtMaxStrategyValue = -1;
            var activeStrategy = new NPCStrategyBuffer();
            for (int i = 0; i < strategyBuffer.Length; i++)
            {
                var strategy = strategyBuffer[i];
                strategy.Active = false;
                if (maxStrategyValue < strategy.StrategyValue)
                {
                    maxStrategyValue = strategy.StrategyValue;
                    indexAtMaxStrategyValue = i;
                    activeStrategy = strategy;
                    activeStrategy.Active = true;
                }
                strategyBuffer[i] = strategy;
            }
            strategyBuffer[indexAtMaxStrategyValue] = activeStrategy;
            movementData.ValueRW.MoveSpeed = activeStrategy.StrategyMoveSpeed;
        }
    }
}