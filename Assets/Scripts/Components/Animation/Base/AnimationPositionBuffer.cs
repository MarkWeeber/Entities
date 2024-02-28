using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

public struct AnimationPositionBuffer : IBufferElementData
{
    public float3 Value;
    public float Time;
}

public struct CompareAnimationPositionTimeBuffer : IComparer<AnimationPositionBuffer>
{
    public int Compare(AnimationPositionBuffer first, AnimationPositionBuffer second)
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