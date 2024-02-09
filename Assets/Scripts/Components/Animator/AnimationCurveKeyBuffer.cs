using Unity.Entities;

[System.Serializable]
public partial struct AnimationCurveKeyBuffer : IBufferElementData
{
    public int Id;
    public int AnimatorInstanceId;
    public int CurveId;
    public float Time;
    public float Value;
}
