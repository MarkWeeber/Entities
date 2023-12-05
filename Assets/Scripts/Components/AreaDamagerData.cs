using Unity.Entities;

public struct AreaDamagerData : IComponentData, IEnableableComponent
{
    public float DamageValue;
    public float DamageTime;
    public float DamageTimer;
}