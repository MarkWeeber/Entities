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
        ProjectileMoveJob projectileMoveJob = new ProjectileMoveJob
        {
            DeltaTime = deltaTime
        };
        JobHandle jobHandle = projectileMoveJob.ScheduleParallel(state.Dependency);
        jobHandle.Complete();
    }
    [BurstCompile]
    private partial struct ProjectileMoveJob : IJobEntity
    {
        public float DeltaTime;
        private void Execute(ProjectileMovementAspect projectileMovementAspect)
        {
            projectileMovementAspect.Move(DeltaTime);
        }
    }
}