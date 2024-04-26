using Unity.Entities;

public struct TransformInstructionController : IComponentData, IEnableableComponent
{
    public bool Completed;
    public int CurrentIndex;
    public bool Looped;
    public bool ReverseAtEnd;
    public float CurrentInstructionTimer;
}