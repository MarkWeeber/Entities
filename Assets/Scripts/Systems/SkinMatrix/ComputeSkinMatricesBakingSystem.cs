using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Entities.Hybrid.Baking;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[BurstCompile]
//[UpdateInGroup(typeof(InitializationSystemGroup))]
//[UpdateBefore(typeof(AnimatorActorBakingSystem))]
public partial struct ComputeSkinMatricesBakingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityQuery deformationEntities = SystemAPI.QueryBuilder()
            .WithAll<DeformationSampleColor, RootEntity, BoneEntity>()
            .WithNone<BoneTag>()
            .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities).Build();
        EntityQuery deformationEntitiesWithAdditionalBakingData = SystemAPI.QueryBuilder()
            .WithAll<DeformationSampleColor, AdditionalEntitiesBakingData>()
            .WithNone<URPMaterialPropertyBaseColor>()
            .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities).Build();
        EntityQuery materialOverrideEntities = SystemAPI.QueryBuilder()
            .WithAll<MaterialOverrideData, Parent, AdditionalEntitiesBakingData>()
            .WithNone<MaterialSineSpeedOverride, LinkedEntityComponent, MaterialHealthRatioOverride>()
            //.WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
            .Build();
        if (deformationEntities.CalculateEntityCount() == 0 && deformationEntitiesWithAdditionalBakingData.CalculateEntityCount() == 0)
        {
            return;
        }
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        EntityCommandBuffer.ParallelWriter parallelWriter = entityCommandBuffer.AsParallelWriter();


        AddBoneAndRootTagsJob addBoneAndRootTagsJob = new AddBoneAndRootTagsJob
        {
            ParallelWriter = parallelWriter
        };
        JobHandle addBoneAndRootTagsJobHandle = addBoneAndRootTagsJob.ScheduleParallel(deformationEntities, state.Dependency);
        addBoneAndRootTagsJobHandle.Complete();

        state.Dependency = new OverrideMaterialColorJob
        {
            ParallelWriter = parallelWriter,
            EntityManager = state.EntityManager
        }.Schedule(deformationEntitiesWithAdditionalBakingData, state.Dependency);

        state.Dependency = new AddOverrideMaterialComponents
        {
            EntityManager = state.EntityManager,
            ECB = entityCommandBuffer
        }.Schedule(materialOverrideEntities, state.Dependency);

        state.Dependency.Complete();

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    [BurstCompile]
    public partial struct AddBoneAndRootTagsJob : IJobEntity
    {
        internal EntityCommandBuffer.ParallelWriter ParallelWriter;
        [BurstCompile]
        private void Execute(
                [ChunkIndexInQuery] int sortKey,
                RefRO<RootEntity> rootEntity,
                in DynamicBuffer<BoneEntity> bones
            )
        {
            // World to local is required for root space conversion of the SkinMatrices
            ParallelWriter.AddComponent(sortKey, rootEntity.ValueRO.Value, new LocalToWorld());
            ParallelWriter.AddComponent(sortKey, rootEntity.ValueRO.Value, new RootTag());

            // Add tags to the bones so we can find them later
            // when computing the SkinMatrices
            for (int boneIndex = 0; boneIndex < bones.Length; ++boneIndex)
            {
                var boneEntity = bones[boneIndex].Value;
                ParallelWriter.AddComponent(sortKey, boneEntity, new BoneTag());
            }
        }
    }

    [BurstCompile]
    public partial struct OverrideMaterialColorJob : IJobEntity
    {
        public EntityManager EntityManager;
        internal EntityCommandBuffer.ParallelWriter ParallelWriter;
        [BurstCompile]
        private void Execute
            (
                [ChunkIndexInQuery] int sortKey,
                RefRO<DeformationSampleColor> deformColor,
                in DynamicBuffer<AdditionalEntitiesBakingData> additionalEntities
            )
        {
            // Override the material color of the deformation materials
            foreach (var rendererEntity in additionalEntities.AsNativeArray())
            {
                if (EntityManager.HasComponent<RenderMesh>(rendererEntity.Value))
                {
                    ParallelWriter.AddComponent(sortKey, rendererEntity.Value, new URPMaterialPropertyBaseColor { Value = deformColor.ValueRO.Value });
                }
            }
        }
    }

    [BurstCompile]
    private partial struct AddOverrideMaterialComponents : IJobEntity
    {
        public EntityManager EntityManager;
        public EntityCommandBuffer ECB;
        [BurstCompile]
        private void Execute(
            RefRO<MaterialOverrideData> materialOverrideData,
            Parent parent,
            in DynamicBuffer<AdditionalEntitiesBakingData> additionalEntities,
            Entity entity
            )
        {
            // Override the sinewave speed color of the deformation materials
            foreach (var rendererEntity in additionalEntities.AsNativeArray())
            {
                if (EntityManager.HasComponent<RenderMesh>(rendererEntity.Value))
                {
                    ECB.AddComponent(rendererEntity.Value, new MaterialSineSpeedOverride
                    {
                        Value = materialOverrideData.ValueRO.SineSpeed,
                    });
                    ECB.AddComponent(rendererEntity.Value, new MaterialHealthRatioOverride
                    {
                        Value = materialOverrideData.ValueRO.HealthRatio,
                    });
                    ECB.AddComponent(rendererEntity.Value, new LinkedEntityComponent
                    {
                        Value = parent.Value
                    });
                }
            }
            ECB.RemoveComponent<MaterialOverrideData>(entity);
        }
    }
}