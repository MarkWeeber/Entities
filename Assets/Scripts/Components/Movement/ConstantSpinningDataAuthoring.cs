using Unity.Entities;
using UnityEngine;

public class ConstantSpinningDataAuthoring : MonoBehaviour
{
    public float YSpinAngle = 30f;
    public float HeightPhase = 0.5f;
    public float HeightPhaseSpeed = 5f;

    class Baker : Baker<ConstantSpinningDataAuthoring>
	{
		public override void Bake(ConstantSpinningDataAuthoring authoring)
		{
			Entity entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new ConstantSpinningData
			{
				YSpinAngle = authoring.YSpinAngle,
				HeightPhase = authoring.HeightPhase,
				HeightPhaseSpeed = authoring.HeightPhaseSpeed
			});
		}
	}
}