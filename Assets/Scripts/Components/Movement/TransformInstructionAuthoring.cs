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
					PositionEnabled = item.PositionEnabled,
					AddedPosition = item.AddedPosition,
					RotationEnabled = item.RotationEnabled,
                    AppliedRotation = quaternion.Euler(
                                        math.radians(item.AppliedEulerRotation.x),
                                        math.radians(item.AppliedEulerRotation.y),
                                        math.radians(item.AppliedEulerRotation.z)),
                Timer = 0f
				});

            }
			AddComponent(entity, new TransformInstructionController
			{
				Completed = false,
				CurrentIndex = 0,
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
        public bool PositionEnabled;
        public float3 AddedPosition;
        public bool RotationEnabled;
        public float3 AppliedEulerRotation;
    }
}