using Unity.Entities;
using Unity.Mathematics;

public struct SpawnerComponent : IComponentData
{
    public int Quantity;
    public Entity Prefab;
    public float3 SpawnOriginPosition;
    public float Spacing;
}