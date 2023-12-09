using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct RayCastData : IComponentData
{
    public CollisionFilter CollisionFilter;
    public float3 StartRayOffest;
    public float RayDistance;
    public RaycastHit RaycastHit;
}