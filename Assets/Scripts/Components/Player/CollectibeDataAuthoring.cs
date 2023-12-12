using Unity.Entities;
using UnityEngine;

public class CollectibeDataAuthoring : MonoBehaviour
{

	class Baker : Baker<CollectibeDataAuthoring>
	{
		public override void Bake(CollectibeDataAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new CollectibleData
			{
				CoinsCollected = 0
			});
		}
	}
}