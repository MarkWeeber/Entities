using Unity.Entities;
using Unity.Mathematics;

public struct NPCMovementComponent : IComponentData
{
    public float3 Destination;
    public float MinDistance;
    public float WanderDistance;
    public float MovementSpeedMultiplier;
    public NPCTargetVisionState TargetVisionState;
    public float WaitTimer;
}

public enum NPCTargetVisionState
{
    NonVisible = 0,
    Visible = 1,
    Lost = 2,
}