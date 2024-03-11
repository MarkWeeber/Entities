using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct NPCMovementSystem : ISystem
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
        var npcMovementEntities = SystemAPI.QueryBuilder()
            .WithAll<NPCMovementComponent, MovementData, MovementStatisticData, LocalTransform>()
            .Build();
        if (npcMovementEntities.CalculateEntityCount() < 1)
        {
            return;
        }
        state.Dependency = new NPCMovementJob
        { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel(npcMovementEntities, state.Dependency);
    }

    [BurstCompile]
    private partial struct NPCMovementJob : IJobEntity
    {
        public float DeltaTime;
        [BurstCompile]
        private void Execute(RefRW<LocalTransform> localTransform, RefRW<NPCMovementComponent> npcMovement, RefRO<MovementData> movementData, RefRW<MovementStatisticData> movementStatisticData)
        {
            if (npcMovement.ValueRO.TargetVisionState != NPCTargetVisionState.NonVisible)
            {
                var localPosition = localTransform.ValueRO.Position;
                localPosition.y = 0f;
                var targetPosition = npcMovement.ValueRO.Destination;
                targetPosition.y = 0f;
                var distance = math.distance(targetPosition, localPosition);
                var moveDirection = math.normalize(targetPosition - localPosition);
                moveDirection.y = 0f;
                if (distance > npcMovement.ValueRO.MinDistance) // moving while still distance
                {
                    var speed = npcMovement.ValueRO.MovementSpeedMultiplier * movementData.ValueRO.MoveSpeed;
                    var positionDelta = moveDirection * speed * DeltaTime;
                    var newPosition = positionDelta + localTransform.ValueRO.Position;
                    localTransform.ValueRW.Position = newPosition;
                    movementStatisticData.ValueRW.Speed = speed;
                    movementStatisticData.ValueRW.Velocity = positionDelta;
                }
                else
                {
                    if (npcMovement.ValueRO.TargetVisionState == NPCTargetVisionState.Lost)
                    {
                        npcMovement.ValueRW.TargetVisionState = NPCTargetVisionState.NonVisible;
                    }
                    movementStatisticData.ValueRW.Speed = 0f;
                    movementStatisticData.ValueRW.Velocity = float3.zero;
                }
                var currentRotation = localTransform.ValueRO.Rotation;
                var targetRotation = quaternion.LookRotation(moveDirection, math.up());
                var newRotation = math.slerp(currentRotation, targetRotation, movementData.ValueRO.TurnSpeed * DeltaTime);
                localTransform.ValueRW.Rotation = newRotation;
            }
        }
    }
}