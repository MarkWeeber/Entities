using Unity.Entities;
using UnityEngine;

public class CoinPickUpDataAuthoring : MonoBehaviour
{
	public uint Value = 1;
	class Baker : Baker<CoinPickUpDataAuthoring>
	{
		public override void Bake(CoinPickUpDataAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new CoinPickUpData
			{
				Value = authoring.Value
			});
		}
	}
}