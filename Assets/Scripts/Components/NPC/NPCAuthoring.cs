using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

public class NPCAuthoring : MonoBehaviour
{
    [SerializeField]
    private List<NPCStrategyBuffer> strategies;
    [SerializeField]
    private float TargetReachMinDistance = 1f;
    [SerializeField]
    private float MoveSpeed = 1.0f;
    [SerializeField]
    private float TurnSpeed = 5.0f;
    [SerializeField]
    private Transform visionTransform;
    [SerializeField]
    private float SphereCastRadius = 5f;
    [SerializeField]
    private float2 Fov = new float2(45f, 10f);
    [SerializeField]
    private PhysicsCategoryTags DisregardColliders;
    [SerializeField]
    private float MaxHealth = 100f;
    [SerializeField]
    private float CurrentHealth = 100f;
    [SerializeField]
    private GameObject AttackingSphere;
    [SerializeField]
    private float AttackDamage = 5f;
    [SerializeField]
    private float AttackRate = 1f;
    [SerializeField]
    private float AttackRadius = 1f;
    [SerializeField]
    private PhysicsCategoryTags AttackTargetLayerTag;
    class Baker : Baker<NPCAuthoring>
    {
        public override void Bake(NPCAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            var bufer = AddBuffer<NPCStrategyBuffer>(entity);
            foreach (var item in authoring.strategies)
            {
                bufer.Add(item);
            }
            AddComponent(entity, new MovementData
            {
                MoveSpeed = authoring.MoveSpeed,
                TurnSpeed = authoring.TurnSpeed,
                LockedMovement = float3.zero,
                LockTimer = 0f
            });
            AddComponent(entity, new NPCMovementComponent
            {
                DestinationReached = true,
                Destination = float3.zero,
                TargetReachMinDistance = authoring.TargetReachMinDistance,
                WaitTimer = 0f,
                MovementSpeedMultiplier = 1f,
                TargetVisionState = NPCTargetVisionState.NonVisible
            });
            float3 visionOffset = authoring.visionTransform.position - authoring.transform.position;
            AddComponent(entity, new NPCVisionSettings
            {
                FOV = authoring.Fov,
                SpherCastRadius = authoring.SphereCastRadius,
                VisionOffset = visionOffset,
                DisregardTags = authoring.DisregardColliders
            });
            AddComponent(entity, new MovementStatisticData
            {
                Speed = 0f,
                Velocity = float3.zero
            });
            var seed = (uint)UnityEngine.Random.Range(1, uint.MaxValue);
            AddComponent(entity, new RandomComponent
            {
                Random = new Unity.Mathematics.Random(seed)
            });
            AddComponent(entity, new EnemyTag());
            AddComponent(entity, new HealthData
            {
                CurrentHealth = authoring.CurrentHealth,
                MaxHealth = authoring.MaxHealth
            });
            AddComponent(entity, new NPCAttackingComponent
            {
                AttackingSphereEntity = GetEntity(authoring.AttackingSphere, TransformUsageFlags.Dynamic),
                AttackDamage = authoring.AttackDamage,
                AttackRate = authoring.AttackRate,
                AttackTimer = authoring.AttackRate,
                AttackRadius = authoring.AttackRadius,
                TargetCollider = authoring.AttackTargetLayerTag
            });
        }
    }
}