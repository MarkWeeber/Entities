using Unity.Entities;
using UnityEngine;

public class ItemAuthoring : MonoBehaviour
{
	[SerializeField] private GameObject item;
	class Baker : Baker<ItemAuthoring>
	{
		public override void Bake(ItemAuthoring authoring)
		{
            if (authoring.item.TryGetComponent<IItem>(out IItem _item))
            {
				Entity entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponentObject(entity, new ItemData
				{
					item = _item
				});
			}
		}
	}
}