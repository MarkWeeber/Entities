using System.ComponentModel;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

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
        RayCastJob rayCastJob = new RayCastJob
        {
            CollisionWorld = collisionWorld
        };
        JobHandle jobHandle = rayCastJob.Schedule(state.Dependency);
        jobHandle.Complete();
    }
    [BurstCompile]
    private partial struct RayCastJob : IJobEntity
    {
        public CollisionWorld CollisionWorld;
        private void Execute(RayCastAspect rayCastAspect, Entity entity)
        {
            rayCastAspect.RayCast(CollisionWorld, entity);
        }
    }
}