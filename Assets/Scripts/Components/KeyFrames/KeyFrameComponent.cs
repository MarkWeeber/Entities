using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[System.Serializable]
public struct KeyFrameComponent : IBufferElementData
{
    public int Index;
    public float Time;
    public float3 Position;
    public float4 Rotation;
    public FixedString32Bytes Name;
}