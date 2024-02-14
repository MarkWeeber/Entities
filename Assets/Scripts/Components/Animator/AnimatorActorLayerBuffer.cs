using Unity.Collections;
using Unity.Entities;

public struct AnimatorActorLayerBuffer : IBufferElementData
{
    public int Id;
    public int CurrentStateId;
    public float CurrentAnimationTime;
    public float TransitionAnimationTime;
}