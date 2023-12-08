using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

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
        
        // sprint abilities
        SprintJob sprintJob = new SprintJob
        {
            Sprinting = playerInputData.Sprinting
        };
        JobHandle sprintJobHandle = sprintJob.ScheduleParallel(state.Dependency);
        sprintJobHandle.Complete();

        // Fire abilities
        if (SystemAPI.TryGetSingleton<ProjectileSpawnerData>(out ProjectileSpawnerData projectileSpawnerData))
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
            FireJob fireJob = new FireJob
            {
                ParallelWriter = parallelWriter,
                Firing = playerInputData.Firing,
                projectilePrefabEnity = projectileSpawnerData.Projectile
            };
            JobHandle fireJobHandle = fireJob.ScheduleParallel(state.Dependency);
            fireJobHandle.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
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
            if (movementData.ValueRO.LockTimer <= 0f) // locker is vacant
            {
                if (Sprinting && sprintAbilityData.ValueRO.Released)
                {
                    movementData.ValueRW.LockTimer = sprintAbilityData.ValueRO.SprintTime;
                    movementData.ValueRW.LockedMovement =
                        localTransform.ValueRO.Forward() * movementData.ValueRO.MoveSpeed * sprintAbilityData.ValueRO.SpeedMultiplier;
                    sprintAbilityData.ValueRW.Active = true;
                    sprintAbilityData.ValueRW.Released = false;
                }
                else if (!sprintAbilityData.ValueRO.Released) // resetting ability
                {
                    sprintAbilityData.ValueRW.Active = false;
                }
                if (!Sprinting && !sprintAbilityData.ValueRO.Released) // release sprint button
                {
                    sprintAbilityData.ValueRW.Released = true;
                }
            }
        }
    }

    [BurstCompile]
    private partial struct FireJob : IJobEntity
    {
        internal EntityCommandBuffer.ParallelWriter ParallelWriter;
        public bool Firing;
        public Entity projectilePrefabEnity;
        private void Execute
            (
                [ChunkIndexInQuery] int sortKey,
                RefRW<MovementData> movementData,
                RefRW<FireAbilityData> fireAbilityData,
                RefRO<LocalTransform> localTransform,
                PlayerTag playerTag
            )
        {
            if (movementData.ValueRO.LockTimer <= 0f) // locker is vacant
            {
                if (Firing && fireAbilityData.ValueRO.Released)
                {
                    movementData.ValueRW.LockTimer = fireAbilityData.ValueRO.FireTime;
                    movementData.ValueRW.LockedMovement = float3.zero;
                    fireAbilityData.ValueRW.Active = true;
                    fireAbilityData.ValueRW.Released = false;
                    float3 spawnLocation = localTransform.ValueRO.Position;
                    float3 forwardDireciton = localTransform.ValueRO.Forward() * fireAbilityData.ValueRO.FirePortForwarDirection;
                    SpawnProjectile(sortKey, spawnLocation, forwardDireciton);
                }
                else if (!fireAbilityData.ValueRO.Released) // resetting ability
                {
                    fireAbilityData.ValueRW.Active = false;
                }
                if (!Firing && !fireAbilityData.ValueRO.Released) // realeasing fire button
                {
                    fireAbilityData.ValueRW.Released = true;
                }
                
            }
        }

        private void SpawnProjectile(int sortkey, float3 spawnLocation, float3 forwardDirection)
        {
            Entity spawnedEntity = ParallelWriter.Instantiate(sortkey, projectilePrefabEnity);
            ParallelWriter.SetComponent(sortkey, spawnedEntity,
                new LocalTransform
                {
                    Position = spawnLocation,
                    Rotation = quaternion.LookRotation(forwardDirection, math.up()),
                    Scale = 1f
                });
        }
    }
}