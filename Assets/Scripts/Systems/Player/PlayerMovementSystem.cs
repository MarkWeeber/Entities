using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.EventSystems;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct PlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerInputData>();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        PlayerInputData playerInputData = SystemAPI.GetSingleton<PlayerInputData>();
        float3 moveDirection = new float3 (playerInputData.MovementVector.x, 0f, playerInputData.MovementVector.y);
        PlayerMoveJob playerMoveJob = new PlayerMoveJob { DeltaTime = deltaTime, MoveDirection = moveDirection };
        JobHandle jobHandle = playerMoveJob.ScheduleParallel(state.Dependency);
        jobHandle.Complete();
       
    }
    [BurstCompile]
    private partial struct PlayerMoveJob : IJobEntity
    {
        public float DeltaTime;
        public float3 MoveDirection;
        [BurstCompile]
        private void Execute(PlayerMovementAspect movementAspect)
        {
            movementAspect.Move(DeltaTime, MoveDirection);
        }
    }
}