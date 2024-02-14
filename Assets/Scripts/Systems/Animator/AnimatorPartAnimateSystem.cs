using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
[UpdateAfter(typeof(AnimatorAnimateSystem))]
public partial struct AnimatorPartAnimateSystem : ISystem
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
        EntityQuery parts = SystemAPI.QueryBuilder().WithAspect<AnimationPartAspect>().Build();
        state.Dependency = new AnimatePartJob().ScheduleParallel(parts, state.Dependency);
    }

    [BurstCompile]
    private partial struct AnimatePartJob : IJobEntity
    {
        private void Execute(AnimationPartAspect aspect)
        {
            aspect.Animate();
        }
    }
}