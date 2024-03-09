using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
            .WithAll<NPCMovementComponent, MovementData, LocalTransform>()
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
        private void Execute(RefRW<LocalTransform> localTransform, RefRW<NPCMovementComponent> npcMovement, RefRO<MovementData> movementData)
        {
            if (npcMovement.ValueRO.IsDestinationSet)
            {
                var localPosition = localTransform.ValueRO.Position;
                var targetPosition = npcMovement.ValueRO.Destination;
                var distance = math.distance(targetPosition, localPosition);
                if (distance > npcMovement.ValueRO.MinDistance)
                {
                    var moveDirection = math.normalize(targetPosition - localPosition);
                    var speed = npcMovement.ValueRO.MovementSpeedMultiplier * movementData.ValueRO.MoveSpeed;
                    var newPosition = moveDirection * speed * DeltaTime + localTransform.ValueRO.Position;
                    var targetRotation = quaternion.LookRotation(moveDirection, math.up());
                    var currentRotation = localTransform.ValueRO.Rotation;
                    var newRotation = math.slerp(currentRotation, targetRotation, movementData.ValueRO.TurnSpeed * DeltaTime);
                    localTransform.ValueRW.Position = newPosition;
                    localTransform.ValueRW.Rotation = newRotation;
                }
                else
                {
                    //npcMovement.ValueRW.IsDestinationSet = false;
                }
            }
        }
    }
}