using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct PlayerAbilitiesSystem : ISystem
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
        PlayerInputData playerInputData = SystemAPI.GetSingleton<PlayerInputData>();
        
        SprintJob sprintJob = new SprintJob
        {
            Sprinting = playerInputData.Sprinting
        };
        JobHandle sprintJobHandle = sprintJob.ScheduleParallel(state.Dependency);
        sprintJobHandle.Complete();

        FireJob fireJob = new FireJob
        {
            Firing = playerInputData.Firing
        };
        JobHandle fireJobHandle = fireJob.ScheduleParallel(state.Dependency);
        fireJobHandle.Complete();
    }

    [BurstCompile]
    private partial struct SprintJob : IJobEntity
    {
        public bool Sprinting;
        private void Execute(
            RefRW<MovementData> movementData,
            RefRW<SprintAbilityData> sprintAbilityData,
            RefRO<LocalTransform> localTransform,
            PlayerTag playerTag)
        {
            if (movementData.ValueRO.LockTimer <= 0f && Sprinting)
            {
                movementData.ValueRW.LockTimer = sprintAbilityData.ValueRO.SprintTime;
                movementData.ValueRW.LocketMovement =
                    localTransform.ValueRO.Forward() * movementData.ValueRO.MoveSpeed * sprintAbilityData.ValueRO.SpeedMultiplier;
                sprintAbilityData.ValueRW.Active = true;
            }
            else if (sprintAbilityData.ValueRO.Active && movementData.ValueRO.LockTimer > 0)
            {
                sprintAbilityData.ValueRW.Active = true;
            }
            else
            {
                sprintAbilityData.ValueRW.Active = false;
            }
        }
    }

    [BurstCompile]
    private partial struct FireJob : IJobEntity
    {
        public bool Firing;
        private void Execute
            (
                RefRW<MovementData> movementData,
                RefRW<FireAbilityData> fireAbilityData,
                RefRO<LocalTransform> localTransform,
                PlayerTag playerTag
            )
        {
            if (!fireAbilityData.ValueRO.Released && Firing)
            {
                fireAbilityData.ValueRW.Released = true;
            }
            if (movementData.ValueRO.LockTimer <= 0f && Firing && fireAbilityData.ValueRO.Released)
            {
                movementData.ValueRW.LockTimer = fireAbilityData.ValueRO.FireTime;
                movementData.ValueRW.LocketMovement = float3.zero;
                fireAbilityData.ValueRW.Active = true;
                fireAbilityData.ValueRW.Released = false;
            }
            else if (fireAbilityData.ValueRO.Active && movementData.ValueRO.LockTimer > 0)
            {
                fireAbilityData.ValueRW.Active = true;
            }
            else
            {
                fireAbilityData.ValueRW.Active = false;
            }
        }
    }
}