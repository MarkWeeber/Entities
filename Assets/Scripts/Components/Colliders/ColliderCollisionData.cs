using Unity.Entities;
using Unity.Physics;

public struct ColliderCollisionData : IComponentData, IEnableableComponent
{
    public bool IsColliding;
    public int CollisionNumber;
    public CollisionFilter CollisionFilter;
    public Entity ParentEntity;
}