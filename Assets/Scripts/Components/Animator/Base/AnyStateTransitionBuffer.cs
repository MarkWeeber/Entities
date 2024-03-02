using Unity.Entities;

[System.Serializable]
public struct AnyStateTransitionBuffer : IBufferElementData
{
    public int Id;
    public int AnimatorInstanceId;
    public int DestinationStateId;
    public bool FixedDuration;
    public float ExitTime;
    public float TransitionDuration;
    public float TransitionOffset;
}