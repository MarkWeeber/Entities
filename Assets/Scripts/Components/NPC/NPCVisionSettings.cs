using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct NPCVisionSettings : IComponentData
{
    public float2 FOV;
    public float SpherCastRadius;
    public float3 Data;
    public float3 VisionOffset;
}