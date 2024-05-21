using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst.Intrinsics;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct SpawnPerInstructionSystem : ISystem
{
    public BufferTypeHandle<SpawnInstructionBuffer> spawnInstructionsTypeHandle;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnInstructionBuffer>();
        spawnInstructionsTypeHandle = state.GetBufferTypeHandle<SpawnInstructionBuffer>();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var entities = SystemAPI.QueryBuilder().WithAll<SpawnInstructionBuffer>().Build();
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        spawnInstructionsTypeHandle.Update(ref state);
        CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        var jobHandle = new SpawnPerInstructionJob
        {
            SpawnInstructionsTypeHandle = spawnInstructionsTypeHandle,
            ECB = ecb,
            CollisionWorld = collisionWorld
        }.Schedule(entities, state.Dependency);
        jobHandle.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    private partial struct SpawnPerInstructionJob : IJobChunk
    {
        public BufferTypeHandle<SpawnInstructionBuffer> SpawnInstructionsTypeHandle;
        public EntityCommandBuffer ECB;
        [ReadOnly]
        public CollisionWorld CollisionWorld;

        [BurstCompile]
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var accessor = chunk.GetBufferAccessor<SpawnInstructionBuffer>(ref SpawnInstructionsTypeHandle);
            for (int i = 0; i < accessor.Length; i++)
            {
                var buffer = accessor[i];
                for (int j = 0; j < buffer.Length; j++)
                {
                    var component = buffer[j];
                    if (!component.Completed)
                    {
                        var instantiatedEntity = ECB.Instantiate(component.Preafab);
                        var setPosition = component.SpawnPosition;
                        if (component.RandomizePositionWithinRange)
                        {
                            GetRandomPosition(ref setPosition, component);
                        }
                        ECB.AddComponent(instantiatedEntity, new LocalTransform
                        {
                            Position = setPosition,
                            Rotation = quaternion.identity,
                            Scale = 1f
                        });
                        component.Completed = true;
                        buffer[j] = component;
                    }
                }
            }
        }

        [BurstCompile]
        private void GetRandomPosition(ref float3 setPosition, SpawnInstructionBuffer instruction)
        {
            var random = Random.CreateFromIndex(instruction.RandomSeed);
            int tries = 10;
            var collisionFilter = new CollisionFilter { BelongsTo = uint.MaxValue, CollidesWith = uint.MaxValue };
            var distanceHits = new NativeList<DistanceHit>(Allocator.Temp);
            while (tries > 0)
            {
                setPosition = random.NextFloat3(instruction.FromRange, instruction.ToRange) + instruction.SpawnPosition;
                if (!CollisionWorld.OverlapSphere(setPosition, instruction.SphereRadius, ref distanceHits, collisionFilter))
                {
                    break;
                }
                tries--;
            }
            distanceHits.Dispose();
        }
    }
}