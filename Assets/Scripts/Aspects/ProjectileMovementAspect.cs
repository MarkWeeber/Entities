using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct ProjectileMovementAspect : IAspect
{
    private readonly Entity entity;
    private readonly RefRW<LocalTransform> localTransform;
    private readonly RefRO<MovementData> movementData;
    private readonly RefRO<ProjectileTag> tag;

    public void Move(float deltaTime)
    {
        float3 movementDirection = 
            localTransform.ValueRO.Forward()
            * deltaTime
            * movementData.ValueRO.MoveSpeed;
        localTransform.ValueRW.Position += movementDirection;
    }
}