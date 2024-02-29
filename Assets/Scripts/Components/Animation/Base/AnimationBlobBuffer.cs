using Unity.Collections;
using Unity.Entities;

public struct AnimationBlobBuffer : IBufferElementData
{
    public int Id;
    public int AnimatorInstanceId;
    public float Length;
    public bool Looped;
    public FixedString32Bytes Name;
    public BlobAssetReference<PathDataPool> PathData;
}

public struct PathDataPool
{
    public BlobArray<PathsPool> PathData;
}

public struct PathsPool
{
    public FixedString512Bytes Path;
    public bool HasPositions;
    public bool HasRotations;
    public bool HasEulerRotations;
    public BlobArray<AnimationPositionBuffer> Positions;
    public BlobArray<AnimationRotationBuffer> Rotations;
    public BlobArray<AnimationRotationBuffer> EulerRotations;
}



public struct RotationsPool
{
    public AnimationRotationBuffer Value;
}
public struct PositionsPool
{
    public AnimationPositionBuffer Value;
}