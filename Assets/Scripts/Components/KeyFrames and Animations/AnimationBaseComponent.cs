using Unity.Collections;
using Unity.Entities;

public struct AnimationBaseComponent : IBufferElementData
{
    public FixedString32Bytes AnimationName;
    public Entity AnimationHolder;
    public float AnimationDuration;
    public bool Loop;
}