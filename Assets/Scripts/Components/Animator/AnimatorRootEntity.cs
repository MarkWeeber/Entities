using Unity.Entities;

public struct AnimatorPartComponent : IComponentData
{
    public Entity RootEntity;
    public int CurrentAnimationPathIndex;
    public int NextAnimationPathIndex;
    public int CurrentAnimationIndex;
    public int NextAnimationIndex;
}