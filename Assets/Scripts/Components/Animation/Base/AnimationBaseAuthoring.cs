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
                    FPS = asset.AnimationClipParsedObject.FPS,
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
                var newBuilder = new BlobBuilder(Allocator.Temp);
                ref PathsPool pathsPool = ref newBuilder.ConstructRoot<PathsPool>();
                pathsPool.Path = (FixedString512Bytes)pathData.Path;
                pathsPool.HasPositions = pathData.HasPosition;
                pathsPool.HasRotations = pathData.HasRotation;
                pathsPool.HasEulerRotations = pathData.HasEulerRotation;
                // positions
                if (pathData.HasPosition)
                {
                    int positionsCount = pathData.Positions.Count;
                    var positionsArrayBuilder = newBuilder.Allocate(ref pathsPool.Positions, positionsCount);
                    for (int k = 0; k < positionsCount; k++)
                    {
                        var listItem = pathData.Positions[k];
                        var newItem = new AnimationPositionBuffer
                        {
                            Time = listItem.Time,
                            Value = listItem.Value
                        };
                        positionsArrayBuilder[k] = newItem;
                    }
                }
                // rotations
                if (pathData.HasRotation)
                {
                    int rotationsCount = pathData.Rotations.Count;
                    var rotationsArrayBuilder = newBuilder.Allocate(ref pathsPool.Rotations, rotationsCount);
                    for (int k = 0; k < rotationsCount; k++)
                    {
                        var listItem = pathData.Rotations[k];
                        var newItem = new AnimationRotationBuffer
                        {
                            Time = listItem.Time,
                            Value = listItem.Value
                        };
                        rotationsArrayBuilder[k] = newItem;
                    }
                }
                // euler rotations
                if (pathData.HasEulerRotation)
                {
                    int eulerRotationsCount = pathData.EulerRotations.Count;
                    var eulerRotaionsArrayBuilder = newBuilder.Allocate(ref pathsPool.EulerRotations, eulerRotationsCount);
                    for (int k = 0; k < eulerRotationsCount; k++)
                    {
                        var listItem = pathData.EulerRotations[k];
                        var newItem = new AnimationRotationBuffer
                        {
                            Time = listItem.Time,
                            Value = listItem.Value
                        };
                        eulerRotaionsArrayBuilder[k] = newItem;
                    }
                }
                // result
                var _result = newBuilder.CreateBlobAssetReference<PathsPool>(Allocator.Persistent);
                pathDataArrayBuilder[i].Path = _result.Value.Path;
                pathDataArrayBuilder[i].HasPositions = _result.Value.HasPositions;
                pathDataArrayBuilder[i].HasRotations = _result.Value.HasRotations;
                pathDataArrayBuilder[i].HasEulerRotations = _result.Value.HasEulerRotations;
                builder.Construct(ref pathDataArrayBuilder[i].Positions, _result.Value.Positions.ToArray());
                builder.Construct(ref pathDataArrayBuilder[i].Rotations, _result.Value.Rotations.ToArray());
                builder.Construct(ref pathDataArrayBuilder[i].EulerRotations, _result.Value.EulerRotations.ToArray());
                newBuilder.Dispose();
            }

            var result = builder.CreateBlobAssetReference<PathDataPool>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }
    }
}