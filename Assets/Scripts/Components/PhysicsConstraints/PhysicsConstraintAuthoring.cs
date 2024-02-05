using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public class PhysicsConstraintAuthoring : MonoBehaviour
{
    public bool3 LinearConstrains;
    public bool3 AngularConstrains;

    class Baker : Baker<PhysicsConstraintAuthoring>
	{
        public override void Bake(PhysicsConstraintAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PhysicsConstraintComponent
            {
                LinearConstrains = authoring.LinearConstrains,
                AngularConstrains = authoring.AngularConstrains
            });
            SetComponentEnabled<PhysicsConstraintComponent>(entity, true);
        }
    }
}