using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[BurstCompile]
[UpdateAfter(typeof(TransformSystemGroup))]
public partial struct PlayerMovementLockSystem : ISystem
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
        float deltaTime = SystemAPI.Time.DeltaTime;
        EntityQuery playerTagQuery = SystemAPI.QueryBuilder().WithAll<PlayerTag, MovementData>().Build();
        new MovementDataTimerJob
        {
            DeltaTime = deltaTime
        }.ScheduleParallel(playerTagQuery);
    }

    [BurstCompile]
    private partial struct MovementDataTimerJob : IJobEntity
    {
        public float DeltaTime;
        [BurstCompile]
        private void Execute (RefRW<MovementData> movementData, PlayerTag playerTag)
        {
            if (movementData.ValueRO.LockTimer > 0f)
            {
                movementData.ValueRW.LockTimer -= DeltaTime;
            }
        }
    }
}