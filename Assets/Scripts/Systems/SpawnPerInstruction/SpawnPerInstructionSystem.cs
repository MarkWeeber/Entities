using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst.Intrinsics;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct SpawnPerInstructionSystem : ISystem
{
    public BufferTypeHandle<SpawnInstructionBuffer> spawnInstructionsTypeHandle;
    public EntityTypeHandle entityTypeHandle;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnInstructionBuffer>();
        spawnInstructionsTypeHandle = state.GetBufferTypeHandle<SpawnInstructionBuffer>();
        entityTypeHandle = state.GetEntityTypeHandle();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        var entities = SystemAPI.QueryBuilder().WithAll<SpawnInstructionBuffer>().Build();
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        spawnInstructionsTypeHandle.Update(ref state);
        entityTypeHandle.Update(ref state);
        CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        var chunkBaseEntityIndices = entities.CalculateBaseEntityIndexArray(Allocator.TempJob);
        var jobHandle = new SpawnPerInstructionJob
        {
            SpawnInstructionsTypeHandle = spawnInstructionsTypeHandle,
            ECB = ecb,
            CollisionWorld = collisionWorld,
            ETH = entityTypeHandle,
            ChunkBaseEntityIndices = chunkBaseEntityIndices
        }.Schedule(entities, state.Dependency);
        jobHandle.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        chunkBaseEntityIndices.Dispose();
    }

    [BurstCompile]
    private partial struct SpawnPerInstructionJob : IJobChunk
    {
        public NativeArray<int> ChunkBaseEntityIndices;
        public BufferTypeHandle<SpawnInstructionBuffer> SpawnInstructionsTypeHandle;
        public EntityCommandBuffer ECB;
        public EntityTypeHandle ETH;
        [ReadOnly]
        public CollisionWorld CollisionWorld;

        [BurstCompile]
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            int baseEntityIndex = ChunkBaseEntityIndices[unfilteredChunkIndex];
            Debug.Log($"baseEntityIndex = {baseEntityIndex}");
            return;
            var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
            while (enumerator.NextEntityIndex(out var i))
            {
                Debug.Log($"i = {i}");
            }
            var array = chunk.GetNativeArray(ETH);
            foreach (var item in array)
            {
                Debug.Log($"item.Index = {item.Index}, unfilteredChunkIndex = {unfilteredChunkIndex}");
            }
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
            var random = Unity.Mathematics.Random.CreateFromIndex(instruction.RandomSeed);
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