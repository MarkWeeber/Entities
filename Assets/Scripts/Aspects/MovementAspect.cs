using System.Threading;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Aspects;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;

public readonly partial struct PlayerMovementAspect : IAspect
{
    private readonly Entity _entity;
    private readonly RefRW<LocalTransform> localTransform;
    private readonly RefRO<MovementData> movementData;
    private readonly RefRW<MovementStatisticData> movementStatisticData;
    private readonly RigidBodyAspect _rigidBodyAspect;
    public void MoveBySettingVelocity(float deltaTime, float3 moveDirection)
    {
        float3 velocity = moveDirection * movementData.ValueRO.MoveSpeed;
        SetVelocity(deltaTime, velocity);
        SlerpRotate(moveDirection, deltaTime);
        ResetRotation();
        ResetAngularVelocity();
        SaveStatistics(velocity);
    }

    private void SetVelocity(float deltaTime, float3 velocity)
    {
        velocity.y = _rigidBodyAspect.LinearVelocity.y;
        _rigidBodyAspect.LinearVelocity = velocity;
    }

    private void SlerpRotate(float3 moveDirection, float deltaTime)
    {
        if (math.lengthsq(moveDirection) > float.Epsilon)
        {
            quaternion targetRotation = quaternion.LookRotation(moveDirection, math.up());
            quaternion currentRotation = localTransform.ValueRO.Rotation;
            quaternion newRotation = math.slerp(currentRotation, targetRotation, movementData.ValueRO.TurnSpeed * deltaTime);
            localTransform.ValueRW.Rotation = newRotation;
        }
    }

    private void ResetRotation()
    {
        quaternion newCurrentRotation = localTransform.ValueRO.Rotation;
        newCurrentRotation.value.x = 0f;
        newCurrentRotation.value.z = 0f;
        localTransform.ValueRW.Rotation = newCurrentRotation;
    }

    private void ResetAngularVelocity()
    {
        _rigidBodyAspect.AngularVelocityLocalSpace = float3.zero;
        _rigidBodyAspect.AngularVelocityWorldSpace = float3.zero;
    }

    private void SaveStatistics(float3 velocity)
    {
        float3 velocityWithInput = velocity * movementData.ValueRO.MoveSpeed * velocity;
        movementStatisticData.ValueRW.Velocity = velocityWithInput;
        movementStatisticData.ValueRW.Speed = math.length(velocityWithInput);
    }
}