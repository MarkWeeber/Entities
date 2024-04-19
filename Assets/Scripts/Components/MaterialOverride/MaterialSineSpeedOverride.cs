using Unity.Entities;
using Unity.Rendering;

[MaterialProperty("_SineSpeed")]
public struct MaterialSineSpeedOverride : IComponentData
{
    public float Value;
}