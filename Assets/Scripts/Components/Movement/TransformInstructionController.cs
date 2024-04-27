using Unity.Entities;

public struct TransformInstructionController : IComponentData, IEnableableComponent
{
    public bool Completed;
    public int CurrentInstructionIndex;
    public bool Looped;
    public bool ReverseAtEnd;
    public float CurrentInstructionTimer;
}