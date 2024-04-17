using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class BlobArrayAuthoring : MonoBehaviour
{
    [SerializeField] private MyRootObject myRootObject;
    class Baker : Baker<BlobArrayAuthoring>
    {
        public override void Bake(BlobArrayAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var data = CreateRootObjectBlobAsset(authoring.myRootObject);
            AddComponent(entity, new SomeRootComponent
            {
                Value = data
            });
        }
        private BlobAssetReference<SomeRootObject> CreateRootObjectBlobAsset(MyRootObject myRootObject)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref SomeRootObject pool = ref builder.ConstructRoot<SomeRootObject>();
            var rootCount = myRootObject.MyAttributes.Count;
            var attributeSetsBuilder = builder.Allocate(ref pool.AttributSets, rootCount);
            for (int i = 0; i < rootCount; i++)
            {
                var rootItem = myRootObject.MyAttributes[i];
                var subBuilder = new BlobBuilder(Allocator.Temp);
                ref AttributeSet attributesPool = ref subBuilder.ConstructRoot<AttributeSet>();
                int subCount = rootItem.Attributes.Count;
                var attributesBuilder = subBuilder.Allocate(ref attributesPool.Attributes, subCount);
                for (int k = 0; k < subCount; k++)
                {
                    attributesBuilder[k].name = (FixedString512Bytes)rootItem.Attributes[k].Name;
                }
                var _result = subBuilder.CreateBlobAssetReference<AttributeSet>(Allocator.Persistent);
                attributeSetsBuilder[i].name = (FixedString512Bytes)rootItem.Name;
                builder.Construct(ref attributeSetsBuilder[i].Attributes, _result.Value.Attributes.ToArray());
                subBuilder.Dispose();
            }
            var result = builder.CreateBlobAssetReference<SomeRootObject>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }
    }
}

public struct SomeRootObject
{
    public BlobArray<AttributeSet> AttributSets;
}
public struct AttributeSet
{
    public FixedString512Bytes name;

    public BlobArray<Attribute> Attributes;
}

public struct Attribute
{
    public FixedString512Bytes name;
}

public struct SomeRootComponent : IComponentData
{
    public BlobAssetReference<SomeRootObject> Value;
}


[System.Serializable]
public struct MyRootObject
{
    public List<MyAttributeSet> MyAttributes;
}

[System.Serializable]
public struct MyAttributeSet
{
    public string Name;
    public List<MyAttribute> Attributes;
}

[System.Serializable]
public struct MyAttribute
{
    public string Name;
}