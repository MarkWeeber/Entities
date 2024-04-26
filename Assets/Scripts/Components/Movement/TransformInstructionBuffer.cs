using Unity.Entities;
using Unity.Mathematics;

public struct TransformInstructionBuffer : IBufferElementData
{
    public float Duration;
    public float Timer;
    public bool PositionEnabled;
    public float3 AddedPosition;
    public bool RotationEnabled;
    public quaternion AppliedRotation;
}