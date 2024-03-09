using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct NPCVisionData : IComponentData
{
    public bool IsColliding;
    public int CollisionNumber;
    public CollisionFilter CollisionFilter;
    public float2 FOV;
    public float SpherCastRadius;
    public float3 Data;
    public float3 VisionOffset;
}