using Unity.Collections;
using Unity.Entities;

[System.Serializable]
public partial struct AnimationBuffer : IBufferElementData
{
    public int Id;
    public int AnimatorInstanceId;
    public bool Looped;
    public float Length;
}
