using Unity.Entities;
using UnityEngine;

public class AITagAuthoring : MonoBehaviour
{

	class Baker : Baker<AITagAuthoring>
	{
		public override void Bake(AITagAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new EnemyTag { });
		}
	}
}