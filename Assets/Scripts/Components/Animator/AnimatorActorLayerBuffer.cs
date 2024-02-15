using Unity.Entities;

public struct AnimatorActorLayerBuffer : IBufferElementData
{
    public int Id;
    public float DefaultWeight;

    public int CurrentStateId;
    public float CurrentStateSpeed;
    public int CurrentAnimationId;
    public float CurrentAnimationTime;
    public float CurrentAnimationLength;
    public bool CurrentAnimationIsLooped;


    public bool IsInTransition;
    public float TransitionTimer;
    public float ExitPercentage;
    public bool FixedDuration;
    public float TransitionDuration;
    public float TransitionOffsetPercentage;

    public int NextStateId;
    public float NextStateSpeed;
    public int NextAnimationId;
    public float NextAnimationTime;
    public float NextAnimationLength;
    public bool NextAnimationIsLooped;

    public AnimatorActorLayerBuffer(
        int Id,
        float DefaultWeight,
        int CurrentStateId,
        float CurrentStateSpeed,
        int CurrentAnimationId,
        float CurrentAnimationTime,
        float CurrentAnimationLength,
        bool CurrentAnimationIsLooped,
        bool IsInTransition,
        float TransitionTimer,
        float ExitPercentage,
        bool FixedDuration,
        float TransitionDurationTime,
        float OffsetPercentage,
        int NextStateId,
        float NextStateSpeed,
        int NextAnimationId,
        float NextAnimationTime,
        float NextAnimationLength,
        bool NextAnimationIsLooped
    )
    {
        this.Id = Id;
        this.DefaultWeight = DefaultWeight;
        this.CurrentStateId = CurrentStateId;
        this.CurrentStateSpeed = CurrentStateSpeed;
        this.CurrentAnimationId = CurrentAnimationId;
        this.CurrentAnimationTime = CurrentAnimationTime;
        this.CurrentAnimationLength = CurrentAnimationLength;
        this.CurrentAnimationIsLooped = CurrentAnimationIsLooped;
        this.IsInTransition = IsInTransition;
        this.TransitionTimer = TransitionTimer;
        this.ExitPercentage = ExitPercentage;
        this.FixedDuration = FixedDuration;
        this.TransitionDuration = TransitionDurationTime;
        this.TransitionOffsetPercentage = OffsetPercentage;
        this.NextStateId = NextStateId;
        this.NextStateSpeed = NextStateSpeed;
        this.NextAnimationId = NextAnimationId;
        this.NextAnimationTime = NextAnimationTime;
        this.NextAnimationLength = NextAnimationLength;
        this.NextAnimationIsLooped = NextAnimationIsLooped;
    }
    public AnimatorActorLayerBuffer(AnimatorActorLayerBuffer layer)
    {
        this.Id = layer.Id;
        this.DefaultWeight = layer.DefaultWeight;
        this.CurrentStateId = layer.CurrentStateId;
        this.CurrentStateSpeed = layer.CurrentStateSpeed;
        this.CurrentAnimationId = layer.CurrentAnimationId;
        this.CurrentAnimationTime = layer.CurrentAnimationTime;
        this.CurrentAnimationLength = layer.CurrentAnimationLength;
        this.CurrentAnimationIsLooped = layer.CurrentAnimationIsLooped;
        this.IsInTransition = layer.IsInTransition;
        this.TransitionTimer = layer.TransitionTimer;
        this.ExitPercentage = layer.ExitPercentage;
        this.FixedDuration = layer.FixedDuration;
        this.TransitionDuration = layer.TransitionDuration;
        this.TransitionOffsetPercentage = layer.TransitionOffsetPercentage;
        this.NextStateId = layer.NextStateId;
        this.NextStateSpeed = layer.NextStateSpeed;
        this.NextAnimationId = layer.NextAnimationId;
        this.NextAnimationTime = layer.NextAnimationTime;
        this.NextAnimationLength = layer.NextAnimationLength;
        this.NextAnimationIsLooped = layer.NextAnimationIsLooped;
    }

}