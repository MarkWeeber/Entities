using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TransformInstructionAuthoring : MonoBehaviour
{
	[SerializeField] private List<TransformInstruction> transformInstructions;
	[SerializeField] private bool looped = false;
	[SerializeField] private bool reverseAtEnd = false;
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
			foreach (var item in authoring.transformInstructions)
			{
				instructionBuffer.Add(new TransformInstructionBuffer
				{
					Duration = item.Duration,
					PositionApplied = item.PositionApplied,
					AddedPosition = item.AddedPosition,
					RotationApplied = item.RotationApplied,
                    AppliedRotation = quaternion.Euler(
                                        math.radians(item.AppliedEulerRotation.x),
                                        math.radians(item.AppliedEulerRotation.y),
                                        math.radians(item.AppliedEulerRotation.z)),
					ScalingApplied = item.ScalingApplied,
					TargetScale = item.TargetScale,
					Timer = 0f
				});

            }
			AddComponent(entity, new TransformInstructionController
			{
				Completed = false,
				CurrentInstructionIndex = -1,
				Looped = authoring.looped,
				ReverseAtEnd = authoring.reverseAtEnd,
				CurrentInstructionTimer = 0f,
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
        public float TargetScale;
    }
}