using Unity.Entities;

public struct ProjectileSpawnerData : IComponentData
{
    public Entity Projectile;
    public Entity SpecialProjectile;
}