using Unity.Entities;
using UnityEngine;

public class PlayerTagAuthoring : MonoBehaviour
{
	class Baker : Baker<PlayerTagAuthoring>
	{
		public override void Bake(PlayerTagAuthoring authoring)
		{
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerTag { });
        }
	}
}