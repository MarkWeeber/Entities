using Unity.Collections;
using Unity.Entities;

public struct AnimatorActorPartBufferComponent : IBufferElementData
{
    public Entity Value;
    public FixedString512Bytes Path;
}