using Unity.Collections;
using Unity.Deformations;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Burst;

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateBefore(typeof(DeformationsInPresentation))]
public partial struct CalculateSkinMatrixSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LocalToWorld>();
        state.RequireForUpdate<BoneTag>();
        state.RequireForUpdate<RootTag>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // bones
        EntityQuery boneEntityQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld, BoneTag>().Build();
        int boneCount = boneEntityQuery.CalculateEntityCount();
        NativeParallelHashMap<Entity, float4x4> bonesLocalToWorld = new NativeParallelHashMap<Entity, float4x4>(boneCount, Allocator.TempJob);
        NativeParallelHashMap<Entity, float4x4>.ParallelWriter bonesLocalToWorldParallel = bonesLocalToWorld.AsParallelWriter();
        GatherBoneTransformsJob gatherBoneTransformsJob = new GatherBoneTransformsJob
        {
            BonesLocalToWorldParallel = bonesLocalToWorldParallel
        };
        JobHandle gatherBoneTransformsJobHandle = gatherBoneTransformsJob.ScheduleParallel(state.Dependency);

        // roots
        EntityQuery rootEntityQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld, RootTag>().Build();
        int rootCount = rootEntityQuery.CalculateEntityCount();
        NativeParallelHashMap<Entity, float4x4> rootWorldToLocal = new NativeParallelHashMap<Entity, float4x4>(rootCount, Allocator.TempJob);
        NativeParallelHashMap<Entity, float4x4>.ParallelWriter rootWorldToLocalParallel = rootWorldToLocal.AsParallelWriter();
        GatherRootTransformsJob gatherRootTransformsJob = new GatherRootTransformsJob
        {
            RootWorldToLocalParallel = rootWorldToLocalParallel
        };
        JobHandle gatherRootTransformsJobHandle = gatherRootTransformsJob.ScheduleParallel(state.Dependency);
        state.Dependency = JobHandle.CombineDependencies(gatherBoneTransformsJobHandle, gatherRootTransformsJobHandle);

        // skin matrices
        CalculateSkinMatricesJob calculateSkinMatricesJob = new CalculateSkinMatricesJob
        {
            BonesLocalToWorld = bonesLocalToWorld,
            RootWorldToLocal = rootWorldToLocal
        };
        state.Dependency = calculateSkinMatricesJob.ScheduleParallel(state.Dependency);

        state.Dependency = JobHandle.CombineDependencies(bonesLocalToWorld.Dispose(state.Dependency), rootWorldToLocal.Dispose(state.Dependency));
    }

    [BurstCompile]
    [WithAll(typeof(BoneTag))]
    public partial struct GatherBoneTransformsJob : IJobEntity
    {
        public NativeParallelHashMap<Entity, float4x4>.ParallelWriter BonesLocalToWorldParallel;
        [BurstCompile]
        private void Execute(Entity entity, RefRO<LocalToWorld> localToWorld)
        {
            BonesLocalToWorldParallel.TryAdd(entity, localToWorld.ValueRO.Value);
        }
    }

    [BurstCompile]
    [WithAll(typeof(RootTag))]
    public partial struct GatherRootTransformsJob : IJobEntity
    {
        public NativeParallelHashMap<Entity, float4x4>.ParallelWriter RootWorldToLocalParallel;
        [BurstCompile]
        private void Execute(Entity entity, RefRO<LocalToWorld> localToWorld)
        {
            RootWorldToLocalParallel.TryAdd(entity, math.inverse(localToWorld.ValueRO.Value));
        }
    }

    [BurstCompile]
    public partial struct CalculateSkinMatricesJob : IJobEntity
    {
        [ReadOnly]
        public NativeParallelHashMap<Entity, float4x4> BonesLocalToWorld;
        [ReadOnly]
        public NativeParallelHashMap<Entity, float4x4> RootWorldToLocal;
        [BurstCompile]
        private void Execute
            (
                ref DynamicBuffer<SkinMatrix> skinMatrices,
                in DynamicBuffer<BindPose> bindPoses,
                in DynamicBuffer<BoneEntity> bones,
                in RootEntity root
            )
        {
            // Loop over each bone
            for (int i = 0; i < skinMatrices.Length; ++i)
            {
                // Grab localToWorld matrix of bone
                var boneEntity = bones[i].Value;
                var rootEntity = root.Value;

                // #TODO: this is necessary for LiveLink?
                if (!BonesLocalToWorld.ContainsKey(boneEntity) || !RootWorldToLocal.ContainsKey(rootEntity))
                    return;

                var matrix = BonesLocalToWorld[boneEntity];

                // Convert matrix relative to root
                var rootMatrixInv = RootWorldToLocal[rootEntity];
                matrix = math.mul(rootMatrixInv, matrix);

                // Compute to skin matrix
                var bindPose = bindPoses[i].Value;
                matrix = math.mul(matrix, bindPose);

                // Assign SkinMatrix
                skinMatrices[i] = new SkinMatrix
                {
                    Value = new float3x4(matrix.c0.xyz, matrix.c1.xyz, matrix.c2.xyz, matrix.c3.xyz)
                };
            }
        }
    }
}