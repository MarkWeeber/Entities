using Unity.Entities;
using UnityEngine;

public class RandomAuthoring : MonoBehaviour
{
	class Baker : Baker<RandomAuthoring>
	{
		public override void Bake(RandomAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.None);
			var seed = (uint)Random.Range(0, 100);
			AddComponent(entity, new RandomComponent
			{
				Random = new Unity.Mathematics.Random(seed)
			});
		}
	}
}