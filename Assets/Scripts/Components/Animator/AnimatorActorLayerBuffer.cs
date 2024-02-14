using Unity.Entities;

public struct AnimatorActorLayerBuffer : IBufferElementData
{
    public int Id;
    public float DefaultWeight;

    public int CurrentStateId;
    public float CurrentStateSpeed;
    public int CurrentAnimationId;
    public float CurrentAnimationTime;
    public bool CurrentAnimationIsLooped;
    

    public bool IsInTransition;
    public float TransitionTimer;
    public float ExitPercentage;
    public bool FixedDuration;
    public float DurationTime;
    public float TransitionAnimationTime;
    public float OffsetPercentage;

    public int NextStateId;
    public float NextStateSpeed;
    public int NextAnimationId;
    public float NextAnimationTime;
    public bool NextAnimationIsLooped;
    
}