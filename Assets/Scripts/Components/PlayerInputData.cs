using Unity.Entities;
using Unity.Mathematics;

public struct PlayerInputData : IComponentData
{
    public bool Firing;
    public bool Sprinting;
    public float2 MovementVector;
}