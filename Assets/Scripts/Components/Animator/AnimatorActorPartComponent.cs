using Unity.Entities;
using Unity.Mathematics;

public struct AnimatorActorPartComponent : IComponentData
{
    public int CurrentAnimationClipId;
    public int NextAnimationClipId;
    public float CurrentAnimationTime;
    public float NextAnimationTime;
    public float TransitionRate;
}