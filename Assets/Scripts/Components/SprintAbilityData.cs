using Unity.Entities;
using Unity.Mathematics;

public struct SprintAbilityData : IComponentData
{
    public float3 SprintDirection;
    public float SpeedMultiplier;
    public float SprintTime;
    public float SprintTimer;
}