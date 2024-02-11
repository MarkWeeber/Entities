using Unity.Collections;
using Unity.Entities;

[System.Serializable]
public partial struct AnimationBuffer : IBufferElementData
{
    public int Id;
    public int AnimatorInstanceId;
    public FixedString32Bytes Name;
    public bool Looped;
}
