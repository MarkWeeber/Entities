using Unity.Collections;
using Unity.Entities;

public partial struct AnimatorBuffer : IBufferElementData
{
    public int Index;
    public int Id;
    public FixedString32Bytes Name;
}
