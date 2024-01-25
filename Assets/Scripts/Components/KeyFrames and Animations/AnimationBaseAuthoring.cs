using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class AnimationBaseAuthoring : MonoBehaviour
{
	public List<AnimationNameAndPrefab> animationNameAndPrefabs = new List<AnimationNameAndPrefab>();
	class Baker : Baker<AnimationBaseAuthoring>
	{
		public override void Bake(AnimationBaseAuthoring authoring)
		{
            Entity entity = GetEntity(TransformUsageFlags.None);
			DynamicBuffer<AnimationBaseComponent> buffer = AddBuffer<AnimationBaseComponent>(entity);
			foreach (AnimationNameAndPrefab item in authoring.animationNameAndPrefabs)
			{
				buffer.Add(new AnimationBaseComponent
				{
					AnimationName = (FixedString32Bytes)item.Name,
					AnimationHolder = GetEntity(item.Prefab, TransformUsageFlags.None),
					AnimationDuration = item.AnimationDuration,
					Loop = item.Loop
				});
            }
        }
	}
	[System.Serializable]
	public struct AnimationNameAndPrefab
	{
		public string Name;
		public GameObject Prefab;
		public float AnimationDuration;
		public bool Loop;
	}
}