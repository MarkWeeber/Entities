using Unity.Mathematics;

public struct AnimationKeyPreProcess
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
