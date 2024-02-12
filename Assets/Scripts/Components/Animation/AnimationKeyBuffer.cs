using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[System.Serializable]
public struct AnimationKeyBuffer : IBufferElementData
{
    public int AnimationId;
    public int AnimatorInstanceId;
    public FixedString512Bytes Path;
    public float Time;
    public bool PositionEngaged;
    public float3 PositionValue;
    public bool RotationEngaged;
    public float4 RotationValue;
    public bool RotationEulerEngaged;
    public float4 RotationEulerValue;
}

[System.Serializable]
public struct AnimationKey
{
    public int AnimationId;
    public int AnimatorInstanceId;
    public string Path;
    public float Time;
    public bool PositionEngaged;
    public float3 PositionValue;
    public bool RotationEngaged;
    public float4 RotationValue;
    public bool RotationEulerEngaged;
    public float4 RotationEulerValue;
}