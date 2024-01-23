using Unity.Collections;
using Unity.Entities;

public struct AnimationPartComponent : IBufferElementData
{
    public FixedString32Bytes Name;
    public Entity Entity;
}