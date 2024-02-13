using Unity.Entities;
using Unity.Mathematics;

public struct AnimationPartRotationBuffer : IBufferElementData
{
    public int AnimationId;
    public quaternion Value;
    public float Time;
}