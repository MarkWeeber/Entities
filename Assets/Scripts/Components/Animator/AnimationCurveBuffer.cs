using Unity.Collections;
using Unity.Entities;

[System.Serializable]
public partial struct AnimationCurveBuffer : IBufferElementData
{
    public int Id;
    public int AnimatorInstanceId;
    public int AnimationId;
    public FixedString512Bytes Path;
    public FixedString32Bytes PropertyName;
}
