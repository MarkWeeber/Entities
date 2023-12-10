using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class FireAbilityDataAuthoring : MonoBehaviour
{
	public Transform FirePort;
	public float FireTime = 0.5f;
	class Baker : Baker<FireAbilityDataAuthoring>
	{
		public override void Bake(FireAbilityDataAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new FireAbilityData
			{
				FirePortOffset = authoring.FirePort.position - authoring.transform.position,
				FirePortForwarDirection = authoring.FirePort.forward,
				FireTime = authoring.FireTime,
				Active = false,
				Released = false,
				FirePortEntity = GetEntity(authoring.FirePort.gameObject, TransformUsageFlags.Dynamic),
				SpecialFireSwitch = false
			});
		}
	}
}