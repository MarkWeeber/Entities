using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct FireAbilityData : IComponentData
{
    public bool Active;
    public float3 FirePortOffset;
    public float3 FirePortForwarDirection;
    public float FireTime;
    public bool Released;
    public Entity FirePortEntity;
}