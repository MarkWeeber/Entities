using Unity.Entities;

[System.Serializable]
public partial struct StateTransitionBuffer : IBufferElementData
{
    public int Id;
    public int AnimatorInstanceId;
    public int StateId;
    public int DestinationStateId;
    public bool FixedDuration;
    public float ExitTime;
    public float TransitionDuration;
    public float TransitionOffset;
    public StateTransitionBuffer(AnyStateTransitionBuffer anyStateTransitionBuffer)
    {
        this.Id = anyStateTransitionBuffer.Id;
        this.AnimatorInstanceId = anyStateTransitionBuffer.AnimatorInstanceId;
        this.StateId = -1;
        this.DestinationStateId = anyStateTransitionBuffer.DestinationStateId;
        this.FixedDuration = anyStateTransitionBuffer.FixedDuration;
        this.ExitTime = anyStateTransitionBuffer.ExitTime;
        this.TransitionDuration = anyStateTransitionBuffer.TransitionDuration;
        this.TransitionOffset = anyStateTransitionBuffer.TransitionOffset;
    }
}
