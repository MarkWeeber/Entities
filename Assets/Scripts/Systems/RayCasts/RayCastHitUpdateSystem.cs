using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[UpdateBefore(typeof(ExportPhysicsWorld))]
public partial struct RayCastHitUpdateSystem : ISystem
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
        CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        EntityQuery raycasterEntity = SystemAPI.QueryBuilder().WithAspect<RayCastAspect>().WithAll<RayCasterTag>().Build();
        new RayCastJob
        {
            CollisionWorld = collisionWorld
        }.ScheduleParallel(raycasterEntity);
    }
    [BurstCompile]
    private partial struct RayCastJob : IJobEntity
    {
        [ReadOnly] public CollisionWorld CollisionWorld;
        [BurstCompile]
        private void Execute(RayCastAspect rayCastAspect, Entity entity)
        {
            rayCastAspect.RayCast(CollisionWorld, entity);
        }
    }
}