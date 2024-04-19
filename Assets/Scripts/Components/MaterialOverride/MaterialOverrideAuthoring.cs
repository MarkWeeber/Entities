using Unity.Entities;
using UnityEngine;

public class MaterialOverrideAuthoring : MonoBehaviour
{
	[SerializeField] private float sineSpeed = 5f;
    [SerializeField] private float healthRatio = 1f;
    class Baker : Baker<MaterialOverrideAuthoring>
	{
		public override void Bake(MaterialOverrideAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Renderable);
			AddComponent(entity, new MaterialOverrideData
            {
				SineSpeed = authoring.sineSpeed,
				HealthRatio = authoring.healthRatio
			});
		}
	}
}