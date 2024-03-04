using Unity.Entities;
using Unity.Physics;

public struct ColliderCollisionData : IComponentData
{
    public bool IsColliding;
    public int CollisionNumber;
    public CollisionFilter CollisionFilter;
}