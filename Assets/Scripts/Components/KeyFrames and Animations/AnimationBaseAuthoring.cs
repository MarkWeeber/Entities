using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics.GraphicsIntegration;
using UnityEngine;

public class AnimationBaseAuthoring : MonoBehaviour
{
    public List<AnimationSetting> AnimationSettings = new List<AnimationSetting>();
	class Baker : Baker<AnimationBaseAuthoring>
	{
		public override void Bake(AnimationBaseAuthoring authoring)
		{
            Entity entity = GetEntity(TransformUsageFlags.None);
			DynamicBuffer<AnimationBaseComponent> buffer = AddBuffer<AnimationBaseComponent>(entity);
			foreach (AnimationSetting item in authoring.AnimationSettings)
            {
                if (item.Prefab.TryGetComponent<KeyFrameAuthoring>(out KeyFrameAuthoring keyFrameAuthoring))
                {
                    if (keyFrameAuthoring.KeyFramesAsset != null)
                    {
                        buffer.Add(new AnimationBaseComponent
                        {
                            AnimationName = (FixedString32Bytes)keyFrameAuthoring.KeyFramesAsset.AnimationName,
                            AnimationHolder = GetEntity(item.Prefab, TransformUsageFlags.None),
                            AnimationDuration = keyFrameAuthoring.KeyFramesAsset.AnimationDuration,
                            TimeScale = item.TimeScale,
                            Loop = item.Loop,
                            EasIn = item.EaseIn
                        });
                    }
                }
            }
        }
	}
}

[System.Serializable]
public struct AnimationSetting
{
    public GameObject Prefab;
    public float TimeScale;
    public bool Loop;
    [Range(0f, 1f)]
    public float EaseIn;
}