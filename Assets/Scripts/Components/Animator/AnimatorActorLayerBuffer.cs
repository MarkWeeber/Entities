using Unity.Collections;
using Unity.Entities;

public struct AnimatorActorLayerBuffer : IBufferElementData
{
    public int Id;
    public int CurrentStateIndex;
    public float AnimationTime;
}