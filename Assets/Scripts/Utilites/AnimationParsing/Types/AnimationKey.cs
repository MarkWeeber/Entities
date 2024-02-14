using Unity.Mathematics;

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
    public quaternion RotationValue;
}