using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct PlayerManagedComponentsSetSystem : ISystem
{
    ComponentLookup<SprintAbilityData> sprintLookup;
    ComponentLookup<FireAbilityData> fireLookup;
    public void OnCreate(ref SystemState state)
    {
        sprintLookup = state.GetComponentLookup<SprintAbilityData>(true);
        fireLookup = state.GetComponentLookup<FireAbilityData>(true);
    }
    public void OnDestroy(ref SystemState state)
    {
    }
    public void OnUpdate(ref SystemState state)
    {
        sprintLookup.Update(ref state);
        fireLookup.Update(ref state);
        foreach (
            (
            RefRO<LocalTransform> localTransform,
            RefRO<MovementStatisticData> movementStatictics,
            RefRO<MovementData> movementData,
            AnimatorManagedComponent animator,
            TransformManagedComponent transform,
            Entity entity
            ) in 
            SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRO<MovementStatisticData>,
                RefRO<MovementData>,
                AnimatorManagedComponent,
                TransformManagedComponent>().WithEntityAccess())
        {
            //transform.Value.position = localTransform.ValueRO.Position;
            //transform.Value.rotation = localTransform.ValueRO.Rotation;
            animator.Value.SetFloat("MoveSpeed", movementStatictics.ValueRO.Speed);
            animator.Value.SetFloat("MoveSpeedMultiplier",
                (movementStatictics.ValueRO.Speed + movementData.ValueRO.MoveSpeed)
                /
                movementData.ValueRO.MoveSpeed);
            if (sprintLookup.TryGetComponent(entity, out SprintAbilityData sprintAbilityData))
            {
                animator.Value.SetBool("Sprinting", sprintAbilityData.Active);
            }
            if (fireLookup.TryGetComponent(entity, out FireAbilityData fireAbilityData))
            {
                animator.Value.SetBool("Firing", fireAbilityData.Active);
            }
        }
    }
}