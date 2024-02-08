using Unity.Entities;
using Unity.Mathematics;

public struct SprintAbilityData : IComponentData
{
    public float SpeedMultiplier;
    public float SprintTime;
    public bool Active;
    public bool Released;
}