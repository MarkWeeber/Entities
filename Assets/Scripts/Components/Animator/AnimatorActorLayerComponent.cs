using Unity.Collections;
using Unity.Entities;

public struct AnimatorActorLayerComponent : IBufferElementData
{
    public int LayerIndex;
    public int CurrentStateIndex;
    public float AnimationTime;
}