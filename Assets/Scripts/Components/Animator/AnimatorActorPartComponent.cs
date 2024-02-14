using Unity.Entities;

public struct AnimatorActorPartComponent : IComponentData
{
    public int CurrentAnimationClipId;
    public int NextAnimationClipId;
    public float CurrentAnimationWeight;
    public float NextAnimationWeight;
    public float CurrentAnimationTime;
    public float NextAnimationTime;
}