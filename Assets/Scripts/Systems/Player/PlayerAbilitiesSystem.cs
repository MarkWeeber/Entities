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
    private ComponentLookup<LocalToWorld> localToWorldLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
        state.RequireForUpdate<PlayerInputData>();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PlayerInputData playerInputData = SystemAPI.GetSingleton<PlayerInputData>();
        EntityQuery playerSprintQuery = SystemAPI.QueryBuilder()
            .WithAll<PlayerTag, MovementData, SprintAbilityData, LocalTransform>()
            .Build();
        EntityQuery playerFireQuery = SystemAPI.QueryBuilder()
            .WithAll<PlayerTag, MovementData, FireAbilityData, LocalTransform>()
            .Build();
        // sprint abilities
        new SprintJob
        {
            Sprinting = playerInputData.Sprinting
        }.ScheduleParallel(playerSprintQuery);

        // Fire abilities
        if (SystemAPI.TryGetSingleton<ProjectileSpawnerData>(out ProjectileSpawnerData projectileSpawnerData))
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            Entity projectilePrefab = projectileSpawnerData.Projectile;
            Entity specialProjectilePrefab = projectileSpawnerData.SpecialProjectile;
            EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
            localToWorldLookup.Update(ref state);
            FireJob fireJob = new FireJob
            {
                ParallelWriter = parallelWriter,
                Firing = playerInputData.Firing,
                ProjectilePrefabEnity = projectilePrefab,
                SpecialProjectilePrefabEnity = specialProjectilePrefab,
                LocalToWorldLookup = localToWorldLookup
            };
            JobHandle fireJobHandle = fireJob.ScheduleParallel(playerFireQuery, state.Dependency);
            fireJobHandle.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [BurstCompile]
    private partial struct SprintJob : IJobEntity
    {
        public bool Sprinting;
        [BurstCompile]
        private void Execute(
            RefRW<MovementData> movementData,
            RefRW<SprintAbilityData> sprintAbilityData,
            RefRO<LocalTransform> localTransform
            )
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
        public Entity ProjectilePrefabEnity;
        public Entity SpecialProjectilePrefabEnity;
        [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
        [BurstCompile]
        private void Execute
            (
                [ChunkIndexInQuery] int sortKey,
                RefRW<MovementData> movementData,
                RefRW<FireAbilityData> fireAbilityData,
                RefRO<LocalTransform> localTransform
            )
        {
            if (movementData.ValueRO.LockTimer <= 0f) // locker is vacant
            {
                if (Firing && (fireAbilityData.ValueRO.Released || true))
                {
                    movementData.ValueRW.LockTimer = fireAbilityData.ValueRO.FireTime;
                    movementData.ValueRW.LockedMovement = float3.zero;
                    fireAbilityData.ValueRW.Active = true;
                    fireAbilityData.ValueRW.Released = false;
                    RefRO<LocalToWorld> firePortLocalToWorld = LocalToWorldLookup.GetRefRO(fireAbilityData.ValueRO.FirePortEntity);
                    float3 spawnLocation = firePortLocalToWorld.ValueRO.Position;
                    quaternion spawnRotation = firePortLocalToWorld.ValueRO.Rotation;
                    SpawnProjectile(sortKey, spawnLocation, spawnRotation, fireAbilityData.ValueRO.SpecialFireSwitch);
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

        private void SpawnProjectile(int sortkey, float3 spawnLocation, quaternion rotation, bool specialFire)
        {
            Entity spawnedEntity = ParallelWriter.Instantiate(sortkey,
                (specialFire) ? SpecialProjectilePrefabEnity : ProjectilePrefabEnity
                );
            ParallelWriter.SetComponent(sortkey, spawnedEntity,
                new LocalTransform
                {
                    Position = spawnLocation,
                    Rotation = rotation,
                    Scale = 1f
                });
        }
    }
}