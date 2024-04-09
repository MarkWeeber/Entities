using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Zenject;

public class PlayerAuthoring : MonoBehaviour
{
    [SerializeField] private float CurrentHealth = 50f;
    [SerializeField] private float MaxHealth = 100f;
    [SerializeField] private float SpeedMultiplier = 1.7f;
    [SerializeField] private float SprintTime = 1f;
    [SerializeField] private float MoveSpeed = 2.0f;
    [SerializeField] private float TurnSpeed = 10.0f;
    [SerializeField] private Transform FirePort;
    [SerializeField] private float FireTime = 0.1f;
    [SerializeField] private bool3 LinearConstrains;
    [SerializeField] private bool3 AngularConstrains = new bool3(true, false, true);

    public PlayerAuthoring(PlayerConfig playerConfig)
    {
        if (playerConfig != null)
        {
            MoveSpeed = playerConfig.MoveSpeed;
            TurnSpeed = playerConfig.TurnSpeed;
            SpeedMultiplier = playerConfig.SpeedMultiplier;
            SprintTime = playerConfig.SprintTime;
        }
        Debug.Log("CREATE IN");
    }

    [Inject]
    public void Construct(PlayerConfig playerConfig)
    {
        if (playerConfig != null)
        {
            MoveSpeed = playerConfig.MoveSpeed;
            TurnSpeed = playerConfig.TurnSpeed;
            SpeedMultiplier = playerConfig.SpeedMultiplier;
            SprintTime = playerConfig.SprintTime;
        }
        Debug.Log("INJECT SUCCESS");
    }

    class Baker : Baker<PlayerAuthoring>
	{
		public override void Bake(PlayerAuthoring authoring)
		{
            //Debug.Log("BAKED");
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new PlayerTag());
            AddComponent(entity, new PlayerManagedComponentSetterTag { });
            AddComponent(entity, new MovementStatisticData { });
            AddComponent(entity, new HealthData
            {
                CurrentHealth = authoring.CurrentHealth,
                MaxHealth = authoring.MaxHealth
            });
            AddComponent(entity, new SprintAbilityData()
            {
                SpeedMultiplier = authoring.SpeedMultiplier,
                SprintTime = authoring.SprintTime,
                Active = false,
                Released = false
            });
            AddComponent(entity, new MovementData
            {
                MoveSpeed = authoring.MoveSpeed,
                TurnSpeed = authoring.TurnSpeed,
                LockedMovement = float3.zero,
                LockTimer = 0f
            });
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
            AddComponent(entity, new CollectibleData
            {
                CoinsCollected = 0
            });
            AddComponent(entity, new PhysicsConstraintComponent
            {
                LinearConstrains = authoring.LinearConstrains,
                AngularConstrains = authoring.AngularConstrains
            });
            SetComponentEnabled<PhysicsConstraintComponent>(entity, true);
        }
	}
}