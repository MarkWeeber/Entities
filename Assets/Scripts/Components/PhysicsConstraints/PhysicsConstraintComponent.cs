using Unity.Entities;
using Unity.Mathematics;

public struct PhysicsConstraintComponent : IComponentData, IEnableableComponent
{
    public bool3 LinearConstrains;
    public bool3 AngularConstrains;
}