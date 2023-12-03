using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class PlayerSetComponentsSystem : SystemBase
{
    private EntityManager entityManager;
    protected override void OnCreate()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
    protected override void OnDestroy()
    {
    }
    protected override void OnUpdate()
    {
        PlayerCharacter playerCharacter = PlayerCharacter.Instance;
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach ((PlayerManagedComponentSetterTag player, Entity entity)
            in SystemAPI.Query<PlayerManagedComponentSetterTag>().WithEntityAccess())
        {
            AnimatorManagedComponent animatorManagedComponent = new AnimatorManagedComponent();
            animatorManagedComponent.Value = playerCharacter.Animator;
            TransformManagedComponent transformManagedComponent = new TransformManagedComponent();
            transformManagedComponent.Value = playerCharacter.transform;
            entityCommandBuffer.AddComponent<AnimatorManagedComponent>(entity, animatorManagedComponent);
            entityCommandBuffer.AddComponent<TransformManagedComponent>(entity, transformManagedComponent);
            entityCommandBuffer.RemoveComponent(entity, typeof(PlayerManagedComponentSetterTag));
            entityCommandBuffer.SetComponent<LocalTransform>(entity,
                new LocalTransform
                {
                    Position = playerCharacter.transform.position,
                    Rotation = playerCharacter.transform.rotation,
                    Scale = 1f
                });
        }
        entityCommandBuffer.Playback(entityManager);
    }
}