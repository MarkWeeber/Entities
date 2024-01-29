using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class KeyFrameAuthoring : MonoBehaviour
{
	public KeyFramesAsset KeyFramesAsset;
	class Baker : Baker<KeyFrameAuthoring>
	{
		public override void Bake(KeyFrameAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.None);
            DynamicBuffer<KeyFrameComponent> keyFramesBuffer = AddBuffer<KeyFrameComponent>(entity);
            KeyFrameStorage keyFrameStorage = new KeyFrameStorage();
			try
			{
                keyFrameStorage = JsonUtility.FromJson<KeyFrameStorage>(authoring.KeyFramesAsset.Content);
				foreach (KeyFrameList keyFrame in keyFrameStorage.Store)
				{
					foreach (KeyFrameComponent item in keyFrame.Keys)
					{
						keyFramesBuffer.Add(item);
                    }
				}
            }
			catch (Exception e)
			{
				Debug.LogException(e);
			}
        }
	}
}