using Unity.Entities;
using Unity.Mathematics;

public struct FireAbilityData : IComponentData
{
    public bool Active;
    public float3 FirePortOffset;
    public float3 FirePortForwarDirection;
    public float FireTime;
    public bool Released;
}