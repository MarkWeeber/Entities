using Unity.Entities;

public struct TransformInstructionController : IComponentData, IEnableableComponent
{
    public bool Completed;
    public bool Looped;
    public float CurrentInstructionTime;
}