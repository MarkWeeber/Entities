using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct AnimationRotationBuffer : IBufferElementData
{
    public int AnimationId;
    public FixedString512Bytes Path;
    public quaternion Value;
    public float Time;
}

public struct CompareAnimationRotationTimeBuffer : IComparer<AnimationRotationBuffer>
{
    public int Compare(AnimationRotationBuffer first, AnimationRotationBuffer second)
    {
        if (first.Time > second.Time)
        {
            return 1;
        }
        if (first.Time < second.Time)
        {
            return -1;
        }
        else
            return 0;
    }
}