using Unity.Entities;
using Unity.Mathematics;

public struct PlayerInputData : IComponentData
{
    public bool Firing;
    public bool Dashing;
    public float2 MovementVector;
}