using Unity.Entities;
using UnityEngine;

public class MovementDataAuthoring : MonoBehaviour
{
	public float MoveSpeed = 1.0f;
	public float TurnSpeed = 5.0f;
	class Baker : Baker<MovementDataAuthoring>
	{
		public override void Bake(MovementDataAuthoring authoring)
		{
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new MovementData
			{
				MoveSpeed = authoring.MoveSpeed,
				TurnSpeed = authoring.TurnSpeed
			});
        }
	}
}