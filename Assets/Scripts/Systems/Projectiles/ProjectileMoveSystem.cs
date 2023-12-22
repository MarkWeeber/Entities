using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct ProjectileMoveSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ProjectileTag>();
        state.RequireForUpdate<MovementData>();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        EntityQuery projectileQuery = SystemAPI.QueryBuilder().WithAspect<ProjectileMovementAspect>().WithAll<ProjectileTag>().Build();
        new ProjectileMoveJob
        {
            DeltaTime = deltaTime
        }.ScheduleParallel(projectileQuery);
    }
    [BurstCompile]
    private partial struct ProjectileMoveJob : IJobEntity
    {
        public float DeltaTime;
        [BurstCompile]
        private void Execute(ProjectileMovementAspect projectileMovementAspect)
        {
            projectileMovementAspect.Move(DeltaTime);
        }
    }
}