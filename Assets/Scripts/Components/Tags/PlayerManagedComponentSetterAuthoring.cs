using Unity.Entities;
using UnityEngine;

public class PlayerManagedComponentSetterAuthoring : MonoBehaviour
{

	class Baker : Baker<PlayerManagedComponentSetterAuthoring>
	{
		public override void Bake(PlayerManagedComponentSetterAuthoring authoring)
		{
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerManagedComponentSetterTag { });
        }
	}
}