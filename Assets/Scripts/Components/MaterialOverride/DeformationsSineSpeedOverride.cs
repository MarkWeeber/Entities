using Unity.Entities;
using Unity.Rendering;

[MaterialProperty("_SineSpeed")]
public struct DeformationsSineSpeedOverride : IComponentData
{
    public float Value;
}