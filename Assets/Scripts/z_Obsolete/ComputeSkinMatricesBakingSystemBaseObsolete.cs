//using Unity.Collections;
//using Unity.Entities;
//using Unity.Entities.Hybrid.Baking;
//using Unity.Rendering;
//using Unity.Transforms;

//[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
//public partial class ComputeSkinMatricesBakingSystemBaseObsolete : SystemBase
//{
//    protected override void OnUpdate()
//    {
//        var ecb = new EntityCommandBuffer(Allocator.TempJob);

//        // This is only executed if we have a valid skinning setup
//        Entities
//            .WithAll<DeformationSampleColor>()
//            .ForEach((Entity entity, in RootEntity rootEntity, in DynamicBuffer<BoneEntity> bones) =>
//            {
//                // World to local is required for root space conversion of the SkinMatrices
//                ecb.AddComponent<LocalToWorld>(rootEntity.Value);
//                ecb.AddComponent<RootTag>(rootEntity.Value);

//                // Add tags to the bones so we can find them later
//                // when computing the SkinMatrices
//                for (int boneIndex = 0; boneIndex < bones.Length; ++boneIndex)
//                {
//                    var boneEntity = bones[boneIndex].Value;
//                    ecb.AddComponent(boneEntity, new BoneTag());
//                }
//            }).WithEntityQueryOptions(EntityQueryOptions.IncludeDisabledEntities).WithoutBurst().WithStructuralChanges().Run();


//        Entities.ForEach((Entity entity, in DeformationSampleColor deformColor, in DynamicBuffer<AdditionalEntitiesBakingData> additionalEntities) =>
//        {
//            // Override the material color of the deformation materials
//            foreach (var rendererEntity in additionalEntities.AsNativeArray())
//            {
//                if (EntityManager.HasComponent<RenderMesh>(rendererEntity.Value))
//                {
//                    ecb.AddComponent(rendererEntity.Value, new URPMaterialPropertyBaseColor { Value = deformColor.Value });
//                }
//            }
//        }).WithEntityQueryOptions(EntityQueryOptions.IncludeDisabledEntities).WithoutBurst().WithStructuralChanges().Run();

//        ecb.Playback(EntityManager);
//        ecb.Dispose();
//    }
//}
