using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;

[InternalBufferCapacity(8)]
public struct AnimationEventBuffer : IBufferElementData
{
    public AnimationEventType EventType;
    public float3 EventPosition;
    public float EventRadius;
    public float EventValue;
    public PhysicsCategoryTags EventCollisionTags;
}

public enum AnimationEventType
{
    None = 0,
    Attack = 1,
    HealUp = 2,
}