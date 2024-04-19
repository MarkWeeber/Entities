using Unity.Entities;
using Unity.Rendering;

[MaterialProperty("_HealthRatio")]
public struct MaterialHealthRatioOverride : IComponentData
{
    public float Value;
}