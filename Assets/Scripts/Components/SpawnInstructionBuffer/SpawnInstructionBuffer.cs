using Unity.Entities;
using Unity.Mathematics;

public struct SpawnInstructionBuffer : IBufferElementData, IEnableableComponent
{
    public bool Completed;
    public Entity Preafab;
    public float3 SpawnPosition;
    public uint RandomSeed;
    public bool RandomizePositionWithinRange;
    public float SphereRadius;
    public float3 FromRange;
    public float3 ToRange;
}