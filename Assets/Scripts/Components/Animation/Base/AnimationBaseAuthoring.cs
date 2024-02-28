using ParseUtils;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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
                    var listItem = pathsPool.Positions[k];
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
                    var listItem = pathsPool.Rotations[i];
                    rotationsArrayBuilder[i] = new AnimationRotationBuffer
                    {
                        Time = listItem.Time,
                        Value = listItem.Value
                    };
                }
                int eulerRotationsCount = pathData.EulerRotations.Count;
                var eulerRotationsArrayBuilder = builder.Allocate(ref pathsPool.EulerRotations, eulerRotationsCount);
                for (int k = 0; k < eulerRotationsCount; k++)
                {
                    var listItem = pathsPool.EulerRotations[k];
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

        //private BlobAssetReference<PathsPool> CreatePathPool(AnimationPathData animationPathData)
        //{
        //    var builder = new BlobBuilder(Allocator.Temp);
        //    ref PathsPool pool = ref builder.ConstructRoot<PathsPool>();
        //    pool.Path = (FixedString512Bytes)animationPathData.Path;
        //    pool.HasPositions = animationPathData.HasPosition;
        //    pool.HasRotations = animationPathData.HasRotation;
        //    pool.HasEulerRotations = animationPathData.HasEulerRotation;
        //    var positionsArrayBuilder = builder.Allocate(ref pool.Positions, animationPathData.Positions.Count);
        //    for (int i = 0; i < animationPathData.Positions.Count; i++)
        //    {
        //        var listItem = animationPathData.Positions[i];
        //        positionsArrayBuilder[i] = new AnimationPositionBuffer
        //        {
        //            Time = listItem.Time,
        //            Value = listItem.Value
        //        };
        //    }
        //    var rotationsArrayBuilder = builder.Allocate(ref pool.Rotations, animationPathData.Rotations.Count);
        //    for (int i = 0; i < animationPathData.Rotations.Count; i++)
        //    {
        //        var listItem = animationPathData.Rotations[i];
        //        rotationsArrayBuilder[i] = new AnimationRotationBuffer
        //        {
        //            Time = listItem.Time,
        //            Value = listItem.Value
        //        };
        //    }
        //    var eulerRotationsArrayBuilder = builder.Allocate(ref pool.EulerRotations, animationPathData.EulerRotations.Count);
        //    for (int i = 0; i < animationPathData.EulerRotations.Count; i++)
        //    {
        //        var listItem = animationPathData.EulerRotations[i];
        //        eulerRotationsArrayBuilder[i] = new AnimationRotationBuffer
        //        {
        //            Time = listItem.Time,
        //            Value = listItem.Value
        //        };
        //    }
        //    var result = builder.CreateBlobAssetReference<PathsPool>(Allocator.Persistent);
        //    builder.Dispose();
        //    return result;
        //}

        //private BlobAssetReference<PositionsPool> CreatePositionsPool(List<AnimationPositioItem> inputList)
        //{
        //    var builder = new BlobBuilder(Allocator.Temp);

        //    ref PositionsPool pool = ref builder.ConstructRoot<PositionsPool>();
        //    var arrayBuilder = builder.Allocate(ref pool.Positions, inputList.Count);
        //    for (int i = 0; i < inputList.Count; i++)
        //    {
        //        var item = new AnimationPositionBuffer
        //        {
        //            Time = inputList[i].Time,
        //            Value = inputList[i].Value
        //        };
        //        arrayBuilder[i] = item;
        //    }
        //    var result = builder.CreateBlobAssetReference<PositionsPool>(Allocator.Persistent);
        //    builder.Dispose();
        //    return result;
        //}


        //private BlobAssetReference<RotationsPool> CreateRotationsPool(List<AnimationRotationItem> inputList)
        //{
        //    var builder = new BlobBuilder(Allocator.Temp);

        //    ref RotationsPool pool = ref builder.ConstructRoot<RotationsPool>();
        //    var arrayBuilder = builder.Allocate(ref pool.Rotations, inputList.Count);
        //    for (int i = 0; i < inputList.Count; i++)
        //    {
        //        var item = new AnimationRotationBuffer
        //        {
        //            Time = inputList[i].Time,
        //            Value = inputList[i].Value,
        //        };
        //        arrayBuilder[i] = item;
        //    }
        //    var result = builder.CreateBlobAssetReference<RotationsPool>(Allocator.Persistent);
        //    builder.Dispose();
        //    return result;
        //}

        //private BlobAssetReference<RotationsPool> CreateRotationsPool(List<AnimationRotationBuffer> inputList)
        //{
        //    var builder = new BlobBuilder(Allocator.Temp);

        //    ref RotationsPool pool = ref builder.ConstructRoot<RotationsPool>();
        //    var arrayBuilder = builder.Allocate(ref pool.Rotations, inputList.Count);
        //    for (int i = 0; i < inputList.Count; i++)
        //    {
        //        arrayBuilder[i] = inputList[i];
        //    }
        //    var result = builder.CreateBlobAssetReference<RotationsPool>(Allocator.Persistent);
        //    builder.Dispose();
        //    return result;
        //}

        //private BlobAssetReference<PositionsPool> CreatePositionsPool(List<AnimationPositionBuffer> inputList)
        //{
        //    var builder = new BlobBuilder(Allocator.Temp);

        //    ref PositionsPool pool = ref builder.ConstructRoot<PositionsPool>();
        //    var arrayBuilder = builder.Allocate(ref pool.Positions, inputList.Count);
        //    for (int i = 0; i < inputList.Count; i++)
        //    {
        //        arrayBuilder[i] = inputList[i];
        //    }
        //    var result = builder.CreateBlobAssetReference<PositionsPool>(Allocator.Persistent);
        //    builder.Dispose();
        //    return result;
        //}
    }
}