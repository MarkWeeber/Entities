using Unity.Entities;
using Unity.Mathematics;

public struct NPCMovementComponent : IComponentData
{
    public float3 Destination;
    public float MinDistance;
    public float MovementSpeedMultiplier;
    public NPCTargetVisionState TargetVisionState;
}

public enum NPCTargetVisionState
{
    NonVisible = 0,
    Visible = 1,
    Lost = 2,
}