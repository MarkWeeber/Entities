using Unity.Entities;

public struct AnimatorActorLayerBuffer : IBufferElementData
{
    public int Id;
    public float DefaultWeight;

    // current state and animation info
    public int CurrentStateId;
    public float CurrentStateSpeed;
    public int CurrentAnimationBlobIndex;
    public int CurrentAnimationId;
    public float CurrentAnimationTime; // time needed for animation
    public float CurrentAnimationLength;
    public bool CurrentAnimationIsLooped;

    // transition info
    public bool IsInTransition; // main transition switch
    public float TransitionDuration; // actual transition duration
    public float TransitionTimer; // actual transition timer

    public float FirstOffsetTimer; // start offset timer
    public float SecondAnimationOffset; // offset for second animation start
    public float TransitionRate;

    // second state and animation info
    public int NextStateId;
    public float NextStateSpeed;
    public int NextAnimationBlobIndex;
    public int NextAnimationId;
    public float NextAnimationTime; // time needed in transitioning animation
    public float NextAnimationLength;
    public float NextAnimationSpeed;
    public bool NextAnimationIsLooped;
    // animation method
    public PartsAnimationMethod Method;

    public AnimatorActorLayerBuffer(AnimatorActorLayerBuffer layer)
    {
        this.Id = layer.Id;
        this.DefaultWeight = layer.DefaultWeight;
        this.CurrentStateId = layer.CurrentStateId;
        this.CurrentStateSpeed = layer.CurrentStateSpeed;
        this.CurrentAnimationBlobIndex = layer.CurrentAnimationBlobIndex;
        this.CurrentAnimationId = layer.CurrentAnimationId;
        this.CurrentAnimationTime = layer.CurrentAnimationTime;
        this.CurrentAnimationLength = layer.CurrentAnimationLength;
        this.CurrentAnimationIsLooped = layer.CurrentAnimationIsLooped;
        this.IsInTransition = layer.IsInTransition;
        this.TransitionTimer = layer.TransitionTimer;
        this.TransitionDuration = layer.TransitionDuration;
        this.FirstOffsetTimer = layer.FirstOffsetTimer;
        this.SecondAnimationOffset = layer.SecondAnimationOffset;
        this.TransitionRate = layer.TransitionRate;
        this.TransitionDuration = layer.TransitionDuration;
        this.NextStateId = layer.NextStateId;
        this.NextStateSpeed = layer.NextStateSpeed;
        this.NextAnimationBlobIndex = layer.NextAnimationBlobIndex;
        this.NextAnimationId = layer.NextAnimationId;
        this.NextAnimationTime = layer.NextAnimationTime;
        this.NextAnimationLength = layer.NextAnimationLength;
        this.NextAnimationSpeed = layer.NextAnimationSpeed;
        this.NextAnimationIsLooped = layer.NextAnimationIsLooped;
        this.Method = layer.Method;
    }

}

[System.Serializable]
public enum PartsAnimationMethod
{
    Lerp = 0,
    Lean = 1,
    SmoothStep = 2,
}