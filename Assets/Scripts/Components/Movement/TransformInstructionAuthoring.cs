using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TransformInstructionAuthoring : MonoBehaviour
{
	[SerializeField] private List<TransformInstruction> transformInstructions;
	[SerializeField] private bool looped = false;
	class Baker : Baker<TransformInstructionAuthoring>
	{
		public override void Bake(TransformInstructionAuthoring authoring)
		{
			if (authoring.transformInstructions == null || authoring.transformInstructions.Count < 1)
			{
				return;
			}
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			var instructionBuffer = AddBuffer<TransformInstructionBuffer>(entity);
			float instructionTime = 0f;
			foreach (var item in authoring.transformInstructions)
			{
				instructionTime += item.Duration;
                instructionBuffer.Add(new TransformInstructionBuffer
				{
					Duration = item.Duration,
					PositionAdded = item.PositionApplied,
					AddedPosition = item.AddedPosition,
					RotationApplied = item.RotationApplied,
					AppliedEulerRotation = item.AppliedEulerRotation,
                    //AppliedRotation = quaternion.Euler(
                    //                    math.radians(item.AppliedEulerRotation.x),
                    //                    math.radians(item.AppliedEulerRotation.y),
                    //                    math.radians(item.AppliedEulerRotation.z)),
					ScalingApplied = item.ScalingApplied,
					AddedScale = item.AddedScale,
					EndTime = instructionTime
                });

            }
			AddComponent(entity, new TransformInstructionController
			{
				Completed = false,
				Looped = authoring.looped,
				CurrentInstructionTime = 0f,
			});

		}
	}
	[System.Serializable]
	protected struct TransformInstruction
	{
		public float Duration;
        public bool PositionApplied;
        public float3 AddedPosition;
        public bool RotationApplied;
        public float3 AppliedEulerRotation;
		public bool ScalingApplied;
        public float AddedScale;
    }
}