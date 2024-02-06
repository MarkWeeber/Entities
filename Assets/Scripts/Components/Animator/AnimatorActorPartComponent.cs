using Unity.Collections;
using Unity.Entities;

public struct AnimatorActorPartComponent : IBufferElementData
{
    public Entity Value;
    public FixedString512Bytes Path;
}