using Unity.Entities;
using Unity.Mathematics;

public struct TransformInstructionBuffer : IBufferElementData
{
    public float Duration;
    public float Timer;
    public bool PositionApplied;
    public float3 AddedPosition;
    public bool RotationApplied;
    public quaternion AppliedRotation;
    public bool ScalingApplied;
    public float TargetScale;
}