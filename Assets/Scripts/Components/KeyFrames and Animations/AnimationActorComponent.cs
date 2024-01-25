using Unity.Collections;
using Unity.Entities;

public struct AnimationActorComponent : IComponentData
{
    public FixedString32Bytes AnimationName;
    public float AnimationTime;
    public bool NonLoopAnimationReached;
}