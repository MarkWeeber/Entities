using Unity.Collections;
using Unity.Entities;

public struct AnimationBlobBuffer : IBufferElementData
{
    public int Id;
    public float Length;
    public bool Looped;
    public FixedString32Bytes Name;
    public BlobAssetReference<RotationsPool> Rotations;
    public BlobAssetReference<PositionsPool> Position;
}

public struct RotationsPool
{
    public BlobArray<AnimationRotationBuffer> Rotations;
}
public struct PositionsPool
{
    public BlobArray<AnimationPositionBuffer> Positions;
}