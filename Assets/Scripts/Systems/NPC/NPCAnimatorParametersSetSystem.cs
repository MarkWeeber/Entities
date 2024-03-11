using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
[UpdateBefore(typeof(AnimatorAnimateSystem))]
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
             MovementStatisticData>()
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
        private void Execute(ref DynamicBuffer<AnimatorActorParametersBuffer> parameters, RefRO<MovementStatisticData> movementStatisticData)
        {
            // movement parameters
            var moveSpeedParameterName = (FixedString32Bytes)"MoveSpeed";
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.ParameterName == moveSpeedParameterName)
                {
                    parameter.NumericValue = movementStatisticData.ValueRO.Speed;
                    parameters[i] = parameter;
                    break;
                }
            }
        }
    }
}