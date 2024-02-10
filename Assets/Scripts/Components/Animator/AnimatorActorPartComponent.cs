using Unity.Collections;
using Unity.Entities;

public struct AnimatorActorPartComponent : IComponentData
{
    public Entity RootEntity;
    public FixedString512Bytes Path;
}