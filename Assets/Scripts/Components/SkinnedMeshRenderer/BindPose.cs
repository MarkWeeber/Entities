using Unity.Entities;
using Unity.Mathematics;

public struct BindPose : IBufferElementData
{
    public float4x4 Value;
}
