using Unity.Entities;
using UnityEngine;

public class PlayerInputDataAuthoring : MonoBehaviour
{
	class Baker : Baker<PlayerInputDataAuthoring>
	{
		public override void Bake(PlayerInputDataAuthoring authoring)
		{
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerInputData { });
        }
	}
}