using Unity.Entities;
using Unity.Mathematics;

public struct AnimatorActorPartComponent : IComponentData
{
    public int CurrentAnimationClipId;
    public int NextAnimationClipId;
    public float CurrentAnimationTime;
    public float NextAnimationTime;
    public float TransitionRate;
    public float3 FirstPosition;
    public float3 SecondPosition;
    public quaternion FirstRotation;
    public quaternion SecondRotation;
    public bool FirstPosFound;
    public bool SecondPosFound;
    public bool FirstRotFound;
    public bool SecondRotFound;
    public float3 SetPosition;
    public quaternion SetRotation;
}