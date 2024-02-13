using Unity.Entities;
using Unity.Mathematics;

public struct AnimationPartPositionBuffer : IBufferElementData
{
    public int AnimationId;
    public float3 Value;
    public float Time;
}