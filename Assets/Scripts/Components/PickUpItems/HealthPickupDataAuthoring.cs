using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public class HealthPickupDataAuthoring : MonoBehaviour
{
	public float HealAmmount = 15f;

    class Baker : Baker<HealthPickupDataAuthoring>
	{
		public override void Bake(HealthPickupDataAuthoring authoring)
		{
			Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
			AddComponent(entity, new HealthPickupData
			{
				HealAmmount = authoring.HealAmmount
			});
		}
	}
}