using Unity.Entities;
using Unity.Mathematics;

public struct MovementData : IComponentData
{
    public float MoveSpeed;
    public float TurnSpeed;
    public float3 LockedMovement;
    public float LockTimer;
}