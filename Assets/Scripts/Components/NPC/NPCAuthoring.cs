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
    private float MinDistance = 0.2f;
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
                Destination = float3.zero,
                MinDistance = authoring.MinDistance,
                MovementSpeedMultiplier = 1f,
                TargetVisionState = NPCTargetVisionState.NonVisible
            });
            float3 visionOffset = authoring.visionTransform.position - authoring.transform.position;
            AddComponent(entity, new NPCVisionSettings
            {
                FOV = authoring.Fov,
                SpherCastRadius = authoring.SphereCastRadius,
                VisionOffset = visionOffset
            });
            AddComponent(entity, new MovementStatisticData
            {
                Speed = 0f,
                Velocity = float3.zero
            });
        }
    }
}