using ParseUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class AnimationBaseAuthoring : MonoBehaviour
{
    public List<AnimationDotsAsset> Animations;
    class Baker : Baker<AnimationBaseAuthoring>
    {
        public override void Bake(AnimationBaseAuthoring authoring)
        {
            if (authoring.Animations == null)
            {
                return;
            }
            Entity entity = GetEntity(TransformUsageFlags.None);
            RegisterAnimationsWithBlobAssets(entity, authoring);
        }

        private void RegisterAnimationsWithBlobAssets(Entity entity, AnimationBaseAuthoring authoring)
        {
            var animationsWithBlobs = AddBuffer<AnimationBlobBuffer>(entity);
            foreach (var asset in authoring.Animations)
            {
                if (asset == null)
                {
                    continue;
                }
                var pathData = CreateAnimationBlobBuffer(asset.AnimationClipParsedObject);
                animationsWithBlobs.Add(new AnimationBlobBuffer
                {
                    Id = asset.AnimationClipParsedObject.Id,
                    AnimatorInstanceId = asset.AnimationClipParsedObject.AnimatorInstanceId,
                    Length = asset.AnimationClipParsedObject.Length,
                    Looped = asset.AnimationClipParsedObject.Looped,
                    Name = (FixedString32Bytes)asset.AnimationClipParsedObject.AnimationName,
                    PathData = pathData
                });
            }
        }

        private BlobAssetReference<PathDataPool> CreateAnimationBlobBuffer(AnimationClipParsedObject parsedObject)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref PathDataPool pool = ref builder.ConstructRoot<PathDataPool>();
            int pathDataCount = parsedObject.PathData.Count;
            var pathDataArrayBuilder = builder.Allocate(ref pool.PathData, pathDataCount);
            for (int i = 0; i < pathDataCount; i++)
            {
                var pathData = parsedObject.PathData[i];
                ref PathsPool pathsPool = ref builder.ConstructRoot<PathsPool>();
                pathsPool.Path = (FixedString512Bytes)pathData.Path;
                pathsPool.HasPositions = pathData.HasPosition;
                pathsPool.HasRotations = pathData.HasRotation;
                pathsPool.HasEulerRotations = pathData.HasEulerRotation;
                int positionsCount = pathData.Positions.Count;
                var positionsArrayBuilder = builder.Allocate(ref pathsPool.Positions, positionsCount);
                for (int k = 0; k < positionsCount; k++)
                {
                    var listItem = pathData.Positions[k];
                    positionsArrayBuilder[k] = new AnimationPositionBuffer
                    {
                        Time = listItem.Time,
                        Value = listItem.Value
                    };
                }
                int rotationsCount = pathData.Rotations.Count;
                var rotationsArrayBuilder = builder.Allocate(ref pathsPool.Rotations, rotationsCount);
                for(int k = 0;k < rotationsCount; k++)
                {
                    var listItem = pathData.Rotations[k];
                    rotationsArrayBuilder[k] = new AnimationRotationBuffer
                    {
                        Time = listItem.Time,
                        Value = listItem.Value
                    };
                }
                int eulerRotationsCount = pathData.EulerRotations.Count;
                var eulerRotationsArrayBuilder = builder.Allocate(ref pathsPool.EulerRotations, eulerRotationsCount);
                for (int k = 0; k < eulerRotationsCount; k++)
                {
                    var listItem = pathData.EulerRotations[k];
                    eulerRotationsArrayBuilder[k] = new AnimationRotationBuffer
                    {
                        Time = listItem.Time,
                        Value = listItem.Value
                    };
                }
                pathDataArrayBuilder[i] = pathsPool;
            }
            
            var result = builder.CreateBlobAssetReference<PathDataPool>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }
    }
}