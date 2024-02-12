using Unity.Collections;
using Unity.Entities;

[System.Serializable]
public partial struct StateTransitionBuffer : IBufferElementData
{
    public int Id;
    public int AnimatorInstanceId;
    public int StateId;
    public int DestinationStateId;
    public bool HasExitTime;
    public float ExitTime;
    public float TransitionDuration;
    public float TransitionOffset;
}
