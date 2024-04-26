using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct TransformInstructionSystem : ISystem
{
    private ComponentLookup<TransformInstructionController> _componentLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _componentLookup = state.GetComponentLookup<TransformInstructionController>(true);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var entities = SystemAPI.QueryBuilder()
            .WithAll<TransformInstructionBuffer, TransformInstructionController, LocalTransform>().Build();
        if (entities.CalculateEntityCount() < 0)
        {
            return;
        }
        _componentLookup.Update(ref state);
        state.Dependency = new TransformInstructionsJob {
            ComponentLookup = _componentLookup
        }.ScheduleParallel(entities, state.Dependency);

    }

    [BurstCompile]
    private partial struct TransformInstructionsJob : IJobEntity
    {
        [NativeDisableContainerSafetyRestriction] public ComponentLookup<TransformInstructionController> ComponentLookup;
        [BurstCompile]
        private void Execute(
            in DynamicBuffer<TransformInstructionBuffer> instructions,
            RefRW<TransformInstructionController> controller,
            RefRW<LocalTransform> localTransform,
            Entity entity)
        {
            if (controller.ValueRO.Completed || instructions.Length < 1)
            {
                return;
            }
            ComponentLookup.SetComponentEnabled(entity, false);
        }
    }
}