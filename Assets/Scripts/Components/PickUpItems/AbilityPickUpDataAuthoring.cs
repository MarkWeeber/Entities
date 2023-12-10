using Unity.Entities;
using UnityEngine;

public class AbilityPickUpDataAuthoring : MonoBehaviour
{

	class Baker : Baker<AbilityPickUpDataAuthoring>
	{
		public override void Bake(AbilityPickUpDataAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new AbilityPickUpData { });
		}
	}
}