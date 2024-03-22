using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;

public struct NPCVisionSettings : IComponentData
{
    public float2 FOV;
    public float SpherCastRadius;
    public float3 Data;
    public float3 VisionOffset;
    public PhysicsCategoryTags DisregardTags;
}