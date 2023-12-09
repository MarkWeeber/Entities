using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

public class RayCastDataAuthoring : MonoBehaviour
{
	public PhysicsCategoryTags BelongsTo;
    public PhysicsCategoryTags CollidesWith;
	public Vector3 StartRayOffset;
	public float RayDistance = 1f;

    class Baker : Baker<RayCastDataAuthoring>
	{
		public override void Bake(RayCastDataAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			CollisionFilter collisionFilter = new CollisionFilter
			{
				BelongsTo = authoring.BelongsTo.Value,
				CollidesWith = authoring.CollidesWith.Value
			};
			AddComponent(entity, new RayCastData
			{
				StartRayOffest = authoring.StartRayOffset,
				RayDistance = authoring.RayDistance,
				CollisionFilter = collisionFilter,
				RaycastHit = new Unity.Physics.RaycastHit()
			});
			
		}
	}
}