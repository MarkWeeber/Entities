using Unity.Entities;
using Unity.Mathematics;

public struct TransformInstructionController : IComponentData, IEnableableComponent
{
    public bool Completed;
    public bool Looped;
    public float CurrentInstructionTime;
    public float3 TargetPosition;
    public float TargetScale;
    public quaternion TargetRotation;
}