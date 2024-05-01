using Unity.Entities;
using Unity.Mathematics;

public struct TransformInstructionBuffer : IBufferElementData
{
    public float Duration;
    public float EndTime;
    public bool PositionAdded;
    public float3 AddedPosition;
    public bool RotationApplied;
    public float3 AppliedRotation;
    public bool ScalingApplied;
    public float AppliedScale;
}