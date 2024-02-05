using Unity.Collections;
using Unity.Entities;

public struct AnimatorActorLayerComponent : IBufferElementData
{
    public int LayerNumber;
    public int CurrentStateIndex;
    public float AnimationTime;
    public Entity LayerEntity;
}