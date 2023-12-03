using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

public partial struct PlayerManagedComponentsSetSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }
    public void OnDestroy(ref SystemState state)
    {
    }
    public void OnUpdate(ref SystemState state)
    {
        foreach ( (RefRO<LocalTransform> localTransform, RefRO<MovementStatisticData> moveData, AnimatorManagedComponent animator, TransformManagedComponent transform) in 
            SystemAPI.Query<RefRO<LocalTransform>, RefRO<MovementStatisticData>, AnimatorManagedComponent, TransformManagedComponent>())
        {
            transform.Value.position = localTransform.ValueRO.Position;
            transform.Value.rotation = localTransform.ValueRO.Rotation;
            animator.Value.SetFloat("MoveSpeed", moveData.ValueRO.Speed);
        }
    }
}