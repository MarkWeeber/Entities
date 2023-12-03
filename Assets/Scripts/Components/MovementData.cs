using Unity.Entities;

public struct MovementData : IComponentData
{
    public float MoveSpeed;
    public float TurnSpeed;
}