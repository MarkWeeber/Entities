using Unity.Entities;
using Unity.Physics.Authoring;

public struct NPCAttackingComponent : IComponentData
{
    public Entity AttackingSphereEntity;
    public float AttackDamage;
    public float AttackRate;
    public float AttackTimer;
    public float AttackRadius;
    public PhysicsCategoryTags TargetCollider;
}