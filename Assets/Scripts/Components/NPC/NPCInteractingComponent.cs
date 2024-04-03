using Unity.Entities;
using Unity.Physics.Authoring;

public struct NPCInteractingComponent : IComponentData
{
    public Entity InteractingCastSphere;
    public float InteractValue;
    public float InteractRadius;
}