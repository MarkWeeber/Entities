using Unity.Entities;
using UnityEngine;

public class AreaDamagerDataAuthoring : MonoBehaviour
{
	public float DamageValue = 10f;
	public float DamageTime = 1.5f;

	class Baker : Baker<AreaDamagerDataAuthoring>
	{
		public override void Bake(AreaDamagerDataAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new AreaDamagerData
			{
				DamageValue = authoring.DamageValue,
				DamageTime = authoring.DamageTime,
				DamageTimer = authoring.DamageTime
            });
		}
	}
}