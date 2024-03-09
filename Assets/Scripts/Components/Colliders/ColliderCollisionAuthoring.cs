using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

public class ColliderCollisionAuthoring : MonoBehaviour
{
    public GameObject Parent;
    public PhysicsCategoryTags BelongsTo;
    public PhysicsCategoryTags CollidesWith;
    class Baker : Baker<ColliderCollisionAuthoring>
	{
		public override void Bake(ColliderCollisionAuthoring authoring)
		{
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            CollisionFilter collisionFilter = new CollisionFilter
            {
                BelongsTo = authoring.BelongsTo.Value,
                CollidesWith = authoring.CollidesWith.Value
            };
            Entity parentEntity = GetEntity(authoring.Parent, TransformUsageFlags.Dynamic);
            AddComponent(entity, new ColliderCollisionData
            {
                CollisionFilter = collisionFilter,
                CollisionNumber = 0,
                IsColliding = false,
                ParentEntity = parentEntity
            });
        }
	}
}