using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct SpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnerComponent>();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton<SpawnerComponent>(out SpawnerComponent spawnerComponent))
        {
            Entity prefabEntity = spawnerComponent.Prefab;
            var query = SystemAPI.QueryBuilder().WithAll<SpawnerTag>().Build();
            int existingCount = query.CalculateEntityCount();
            int toBeSpawnAmount = spawnerComponent.Quantity - existingCount;
            if (toBeSpawnAmount > 0)
            {
                EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
                for (int i = 0; i < toBeSpawnAmount; i++)
                {
                    state.Dependency = new SpawnJob
                    {
                        Index = i,
                        ExistingCount = existingCount,
                        Prefab = prefabEntity,
                        OriginPosition = spawnerComponent.SpawnOriginPosition,
                        Spacing = spawnerComponent.Spacing,
                        ECB = ecb
                    }.Schedule(state.Dependency);
                }
                state.Dependency.Complete();
                ecb.Playback(state.EntityManager);
                ecb.Dispose();
            }
        }
    }

    [BurstCompile]
    private partial struct SpawnJob : IJob
    {
        public int ExistingCount;
        public int Index;
        public EntityCommandBuffer ECB;
        public Entity Prefab;
        public float3 OriginPosition;
        public float Spacing;
        [BurstCompile]
        public void Execute()
        {
            Entity entity = ECB.Instantiate(Prefab);
            float3 newPosition = OriginPosition + GetNewPosition(Index + ExistingCount);
            ECB.SetComponent(entity, new LocalTransform
            {
               Position = newPosition,
               Rotation = quaternion.identity,
               Scale = 1f
            });
            ECB.AddComponent(entity, new SpawnerTag());
        }
        [BurstCompile]
        private float3 GetNewPosition(int index)
        {
            int squareRootRange = (int)math.floor(math.sqrt(index)) + 1;
            bool ifEven = squareRootRange % 2 == 0;
            if (ifEven) squareRootRange++;
            int remainder = index & squareRootRange;
            int rowNum = (int)math.floor(index / squareRootRange);
            int colNum = (remainder == squareRootRange) ? squareRootRange - 1 : remainder - 1;
            float shiftCenterValue = squareRootRange * 0.5f * Spacing;
            return new float3(
                rowNum * Spacing - shiftCenterValue,
                0f,
                colNum * Spacing - shiftCenterValue);
        }
    }
}