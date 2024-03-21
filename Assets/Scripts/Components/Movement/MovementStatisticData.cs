using Unity.Entities;
using Unity.Mathematics;

public struct MovementStatisticData : IComponentData
{
    public float Speed;
    public float3 Velocity;
    public bool DestinationReached;
}