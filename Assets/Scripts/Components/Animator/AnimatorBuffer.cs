using Unity.Collections;
using Unity.Entities;

[System.Serializable]
public partial struct AnimatorBuffer : IBufferElementData
{
    public int Id;
    public FixedString32Bytes Name;
}
