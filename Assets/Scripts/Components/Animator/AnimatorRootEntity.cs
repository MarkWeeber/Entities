using Unity.Collections;
using Unity.Entities;

public struct AnimatorPartComponent : IComponentData
{
    public Entity RootEntity;
    public int PathAnimationBlobIndex;
    public FixedString512Bytes PartName;
}