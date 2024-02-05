using Unity.Collections;
using Unity.Entities;

public struct AnimatorActorComponent : IComponentData, IEnableableComponent
{
    public FixedString32Bytes AnimatorControllerName;
    public Entity AnimatorControllerEntity;
}