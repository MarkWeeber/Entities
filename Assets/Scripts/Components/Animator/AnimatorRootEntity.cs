using Unity.Entities;

public struct AnimatorPartComponent : IComponentData
{
    public Entity RootEntity;
    public int PathAnimationBlobIndex;
}