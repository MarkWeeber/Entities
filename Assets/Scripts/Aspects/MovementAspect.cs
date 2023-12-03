using System.Threading;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct PlayerMovementAspect : IAspect
{
    private readonly Entity _entity;
    private readonly RefRW<LocalTransform> localTransform;
    private readonly RefRO<MovementData> movementData;
    private readonly RefRW<MovementStatisticData> movementStatisticData;
    public void Move(float deltaTime, float3 moveDirection)
    {
        float3 velocity = moveDirection * deltaTime * movementData.ValueRO.MoveSpeed;
        localTransform.ValueRW.Position += velocity;
        if (math.lengthsq(moveDirection) > float.Epsilon)
        {
            quaternion targetRotation = quaternion.LookRotation(moveDirection, math.up());
            quaternion currentRotation = localTransform.ValueRO.Rotation;
            quaternion newRotation = math.slerp(currentRotation, targetRotation, movementData.ValueRO.TurnSpeed * deltaTime);
            localTransform.ValueRW.Rotation = newRotation;
        }
        movementStatisticData.ValueRW.Velocity = velocity;
        movementStatisticData.ValueRW.Speed = math.length(velocity);
    }
}