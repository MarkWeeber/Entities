using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class NPCAuthoring : MonoBehaviour
{
	[SerializeField]
	private List<NPCStrategyBuffer> strategies;
	[SerializeField]
	private float MinDistance = 0.2f;
	[SerializeField]
	public float MoveSpeed = 1.0f;
	[SerializeField]
	public float TurnSpeed = 5.0f;
	class Baker : Baker<NPCAuthoring>
	{
		public override void Bake(NPCAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			var bufer = AddBuffer<NPCStrategyBuffer>(entity);
			foreach (var item in authoring.strategies)
			{
				bufer.Add(item);
			}
			AddComponent(entity, new MovementData
			{
				MoveSpeed = authoring.MoveSpeed,
				TurnSpeed = authoring.TurnSpeed,
				LockedMovement = float3.zero,
				LockTimer = 0f
			});
			AddComponent(entity, new NPCMovementComponent
			{
				IsDestinationSet = false,
				Destination = float3.zero,
				MinDistance = authoring.MinDistance,
				MovementSpeedMultiplier = 1f
			});
		}
	}
}