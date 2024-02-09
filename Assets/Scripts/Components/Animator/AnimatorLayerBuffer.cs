using Unity.Entities;

[System.Serializable]
public partial struct AnimatorLayerBuffer : IBufferElementData
{
    public int Id;
    public int AnimatorInstanceId;
    public float DefaultWeight;
}
