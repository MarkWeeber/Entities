using Unity.Entities;

public struct AnimatorActorTransitionComponent : IBufferElementData
{
    public int LayerIndex;
    public bool Running;
    public bool HasExitTime;
    public float TransitionTimer;
    public float TransitionDuration;
    public float ExitTimeDuration;
    public float OffsetTimeDuration;
    public int CurrentStateIndex;
    public int NextStateIndex;
}