using Unity.Collections;
using Unity.Entities;

[System.Serializable]
public partial struct LayerStateBuffer : IBufferElementData
{
    public int Id;
    public int AnimatorInstanceId;
    public int LayerId;
    public bool DefaultState;
    public int AnimationClipId;
    public float Speed;
}
