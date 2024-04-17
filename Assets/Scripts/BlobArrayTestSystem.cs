using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct BlobArrayTestSystem : ISystem
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
        state.Enabled = false;

        foreach (SomeRootComponent rootComponent in SystemAPI.Query<SomeRootComponent>())
        {
            ref var someRootObject = ref rootComponent.Value.Value;
            for (int i = 0; i < someRootObject.AttributSets.Length; i++)
            {
                ref var attributeSet = ref someRootObject.AttributSets[i];
                Debug.Log(attributeSet.name);
                for (int j = 0; j < attributeSet.Attributes.Length; j++)
                {
                    ref var attribute = ref attributeSet.Attributes[j];
                    Debug.Log(attribute.name);
                }
            }
        }
    }
}