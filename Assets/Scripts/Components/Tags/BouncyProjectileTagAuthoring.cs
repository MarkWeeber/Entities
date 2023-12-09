using Unity.Entities;
using UnityEngine;

public class BouncyProjectileTagAuthoring : MonoBehaviour
{

	class Baker : Baker<BouncyProjectileTagAuthoring>
	{
		public override void Bake(BouncyProjectileTagAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new BouncyProjectileTag { });
		}
	}
}