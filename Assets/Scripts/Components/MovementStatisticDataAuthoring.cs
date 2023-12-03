using Unity.Entities;
using UnityEngine;

public class MovementStatisticDataAuthoring : MonoBehaviour
{

	class Baker : Baker<MovementStatisticDataAuthoring>
	{
		public override void Bake(MovementStatisticDataAuthoring authoring)
		{
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MovementStatisticData { });
        }
	}
}