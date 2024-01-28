using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class DeformationsSampleAuthoring : MonoBehaviour
{
    [Tooltip("Override the color in Deformation Material")]
    public Color Color = new Color(.9f, .3f, .5f);
    class Baker : Baker<DeformationsSampleAuthoring>
	{
		public override void Bake(DeformationsSampleAuthoring authoring)
		{
            var skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>(authoring);
            if (skinnedMeshRenderer == null)
                return;

            if (skinnedMeshRenderer.sharedMesh == null)
                return;

            var c = authoring.Color.linear;
            var color = new float4(c.r, c.g, c.b, c.a);
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DeformationSampleColor { Value = color });

            // Only execute this if we have a valid skinning setup
            DependsOn(skinnedMeshRenderer.sharedMesh);
            var hasSkinning = skinnedMeshRenderer.bones.Length > 0 && skinnedMeshRenderer.sharedMesh.bindposes.Length > 0;
            if (hasSkinning)
            {
                // Setup reference to the root bone
                var rootTransform = skinnedMeshRenderer.rootBone ? skinnedMeshRenderer.rootBone : skinnedMeshRenderer.transform;
                var rootEntity = GetEntity(rootTransform, TransformUsageFlags.Dynamic);
                AddComponent(entity, new RootEntity { Value = rootEntity });

                // Setup reference to the other bones
                var boneEntityArray = AddBuffer<BoneEntity>(entity);
                boneEntityArray.ResizeUninitialized(skinnedMeshRenderer.bones.Length);

                for (int boneIndex = 0; boneIndex < skinnedMeshRenderer.bones.Length; ++boneIndex)
                {
                    var bone = skinnedMeshRenderer.bones[boneIndex];
                    var boneEntity = GetEntity(bone, TransformUsageFlags.Dynamic);
                    boneEntityArray[boneIndex] = new BoneEntity { Value = boneEntity };
                }

                // Store the bindpose for each bone
                var bindPoseArray = AddBuffer<BindPose>(entity);
                bindPoseArray.ResizeUninitialized(skinnedMeshRenderer.bones.Length);

                for (int boneIndex = 0; boneIndex != skinnedMeshRenderer.bones.Length; ++boneIndex)
                {
                    var bindPose = skinnedMeshRenderer.sharedMesh.bindposes[boneIndex];
                    bindPoseArray[boneIndex] = new BindPose { Value = bindPose };
                }
            }
        }
	}
}

internal struct RootEntity : IComponentData
{
    public Entity Value;
}

internal struct BoneEntity : IBufferElementData
{
    public Entity Value;
}

internal struct BindPose : IBufferElementData
{
    public float4x4 Value;
}