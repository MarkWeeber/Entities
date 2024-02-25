using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct AnimatorActorPartBufferComponent : IBufferElementData
{
    public Entity Value;
    public FixedString512Bytes Path;
    public Entity RootEntity;
    public bool SetNewLocalTransform;
    public float3 SetPosition;
    public quaternion SetRotation;
    public float3 SetScale;
}