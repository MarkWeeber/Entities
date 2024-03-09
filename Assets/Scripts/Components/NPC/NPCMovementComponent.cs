using Unity.Entities;
using Unity.Mathematics;

public struct NPCMovementComponent : IComponentData
{
    public bool IsDestinationSet;
    public float3 Destination;
    public float MinDistance;
    public float MovementSpeedMultiplier;
}