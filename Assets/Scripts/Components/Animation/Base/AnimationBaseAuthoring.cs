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
            RegisterRotations(entity, authoring);
            RegisterPositions(entity, authoring);
        }

        private void RegisterAnimationsWithBlobAssets(Entity entity, AnimationBaseAuthoring authoring)
        {
            var animations = AddBuffer<AnimationBuffer>(entity);
            var animationsWithBlobs = AddBuffer<AnimationBlobBuffer>(entity);
            foreach (var asset in authoring.Animations)
            {
                if (asset == null)
                {
                    continue;
                }
                animations.Add(new AnimationBuffer
                {
                    AnimationInstanceId = asset.AnimationClipParsedObject.Id,
                    AnimatorInstanceId = asset.AnimationClipParsedObject.AnimatorInstanceId,
                    Length = asset.AnimationClipParsedObject.Length,
                    Looped = asset.AnimationClipParsedObject.Looped,
                    Name = (FixedString32Bytes)asset.AnimationClipParsedObject.AnimationName,
                });
                var rotationsInputList = new List<AnimationRotationBuffer>();
                foreach (var rotation in asset.AnimationClipParsedObject.Rotations)
                {
                    rotationsInputList.Add(new AnimationRotationBuffer
                    {
                        AnimationId = rotation.AnimationId,
                        Path = (FixedString512Bytes)rotation.Path,
                        Time = rotation.Time,
                        Value = rotation.Value,
                    });
                }

                var positionsInputList = new List<AnimationPositionBuffer>();
                foreach (var position in asset.AnimationClipParsedObject.Positions)
                {
                    positionsInputList.Add(new AnimationPositionBuffer
                    {
                        AnimationId = position.AnimationId,
                        Path = (FixedString512Bytes)position.Path,
                        Time = position.Time,
                        Value = position.Value,
                    });
                }

                var rotationsBlobAssetReference = CreateRotationsPool(rotationsInputList);
                var positionsBlobAssetReference = CreatePositionsPool(positionsInputList);

                animationsWithBlobs.Add(new AnimationBlobBuffer
                {
                    Id = asset.AnimationClipParsedObject.Id,
                    Length = asset.AnimationClipParsedObject.Length,
                    Looped = asset.AnimationClipParsedObject.Looped,
                    Name = (FixedString32Bytes)asset.AnimationClipParsedObject.AnimationName,
                    Position = positionsBlobAssetReference,
                    Rotations = rotationsBlobAssetReference,
                });
            }
        }

        private void RegisterRotations(Entity entity, AnimationBaseAuthoring authoring)
        {
            var rotations = AddBuffer<AnimationRotationBuffer>(entity);
            foreach (var asset in authoring.Animations)
            {
                if (asset == null)
                {
                    continue;
                }
                foreach (var item in asset.AnimationClipParsedObject.Rotations)
                {
                    rotations.Add(new AnimationRotationBuffer
                    {
                        AnimationId = item.AnimationId,
                        Path = (FixedString512Bytes)item.Path,
                        Time = item.Time,
                        Value = item.Value
                    });
                }
            }
        }

        private void RegisterPositions(Entity entity, AnimationBaseAuthoring authoring)
        {
            var positions = AddBuffer<AnimationPositionBuffer>(entity);
            foreach (var asset in authoring.Animations)
            {
                if (asset == null)
                {
                    continue;
                }
                foreach (var item in asset.AnimationClipParsedObject.Positions)
                {
                    positions.Add(new AnimationPositionBuffer
                    {
                        AnimationId = item.AnimationId,
                        Path = (FixedString512Bytes)item.Path,
                        Time = item.Time,
                        Value = item.Value
                    });
                }
            }
        }

        private BlobAssetReference<RotationsPool> CreateRotationsPool(List<AnimationRotationBuffer> inputList)
        {
            var builder = new BlobBuilder(Allocator.Temp);

            ref RotationsPool pool = ref builder.ConstructRoot<RotationsPool>();
            var arrayBuilder = builder.Allocate(ref pool.Rotations, inputList.Count);
            for (int i = 0; i < inputList.Count; i++)
            {
                arrayBuilder[i] = inputList[i];
            }
            var result = builder.CreateBlobAssetReference<RotationsPool>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }

        private BlobAssetReference<PositionsPool> CreatePositionsPool(List<AnimationPositionBuffer> inputList)
        {
            var builder = new BlobBuilder(Allocator.Temp);

            ref PositionsPool pool = ref builder.ConstructRoot<PositionsPool>();
            var arrayBuilder = builder.Allocate(ref pool.Positions, inputList.Count);
            for (int i = 0; i < inputList.Count; i++)
            {
                arrayBuilder[i] = inputList[i];
            }
            var result = builder.CreateBlobAssetReference<PositionsPool>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }
    }
}