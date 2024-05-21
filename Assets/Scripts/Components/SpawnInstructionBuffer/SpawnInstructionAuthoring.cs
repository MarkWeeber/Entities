using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public class SpawnInstructionAuthoring : MonoBehaviour
{
    [SerializeField] private List<SpawnInstruction> spawnInstructions;
    class Baker : Baker<SpawnInstructionAuthoring>
    {
        public override void Bake(SpawnInstructionAuthoring authoring)
        {
            if (!authoring.spawnInstructions.Any())
            {
                return;
            }
            Entity entity = GetEntity(TransformUsageFlags.None);
            var buffer = AddBuffer<SpawnInstructionBuffer>(entity);
            foreach (var spawnInstruction in authoring.spawnInstructions)
            {
                var spawnEntity = GetEntity(spawnInstruction.Prefab, TransformUsageFlags.Dynamic);
                buffer.Add(new SpawnInstructionBuffer
                {
                    Completed = false,
                    Preafab = spawnEntity,
                    SpawnPosition = spawnInstruction.Position,
                    RandomizePositionWithinRange = spawnInstruction.RandomizePosition,
                    SphereRadius = spawnInstruction.SphereRadius,
                    FromRange = spawnInstruction.FromRange,
                    ToRange = spawnInstruction.ToRange,
                    RandomSeed = (uint)Random.Range(0, int.MaxValue)
                });
            }
        }
    }
}

[System.Serializable]
public struct SpawnInstruction
{
    public GameObject Prefab;
    public Vector3 Position;
    public bool RandomizePosition;
    public float SphereRadius;
    public Vector3 FromRange;
    public Vector3 ToRange;
}