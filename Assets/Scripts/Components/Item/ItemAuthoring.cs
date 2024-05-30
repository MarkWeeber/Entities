using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

public class ItemAuthoring : MonoBehaviour
{
	[SerializeField] private GameObject prefab;
	class Baker : Baker<ItemAuthoring>
	{
		public override void Bake(ItemAuthoring authoring)
		{
            if (authoring.gameObject.TryGetComponent<IItem>(out IItem _item))
            {
				_item.InitializeActions();
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
				Entity prefabEntity = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic);
				_item.PrefabEntity = entity;
                AddComponentObject(entity, new ItemData
				{
					Item = _item,
					PrefabEntity = prefabEntity,
				});
			}
		}
	}
}