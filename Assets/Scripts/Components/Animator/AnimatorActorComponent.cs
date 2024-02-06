using Unity.Collections;
using Unity.Entities;

public struct AnimatorActorComponent : IComponentData
{
    public int AnimatorId;
    public FixedString32Bytes AnimatorControllerName;
}