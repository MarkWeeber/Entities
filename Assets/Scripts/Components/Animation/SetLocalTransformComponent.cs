using Unity.Entities;
using Unity.Mathematics;

public struct SetLocalTransformComponent : IComponentData
{
    public bool Set;
    public float3 SetPosition;
    public quaternion SetRotation;
    public float SetScale;
}