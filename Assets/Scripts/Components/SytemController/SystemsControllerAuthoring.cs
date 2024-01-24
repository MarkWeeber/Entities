using Unity.Entities;
using UnityEngine;

public class SystemsControllerAuthoring : MonoBehaviour
{
    public bool Player = true;
    public float PlayerRate = 0f;
    public float PlayerOffset = 0f;
    public bool Animation = true;
    public float AnimationRate = 0f;
    public float AnimationOffset = 0f;
    public bool AI = true;
    public float AIRate = 0f;
    public float AIOffset = 0f;
    public bool AreaEffector = true;
    public float AreaEffectorRate = 0f;
    public float AreaEffectorOffset = 0f;
    public bool Projectile = true;
    public float PojectileRate = 0f;
    public float PojectileOffset = 0f;
    public bool RayCast = true;
    public float RayCastRate = 0f;
    public float RayCastOffset = 0f;

    class Baker : Baker<SystemsControllerAuthoring>
	{
		public override void Bake(SystemsControllerAuthoring authoring)
		{
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new SystemControllerData
            {
                Player = authoring.Player,
                PlayerRate = authoring.PlayerRate,
                PlayerOffset = authoring.PlayerOffset,
                Animation = authoring.Animation,
                AnimationRate = authoring.AnimationRate,
                AnimationOffset = authoring.AnimationOffset,
                AI = authoring.AI,
                AIRate = authoring.AIRate,
                AIOffset = authoring.AIOffset,
                AreaEffector = authoring.AreaEffector,
                AreaEffectorRate = authoring.AreaEffectorRate,
                AreaEffectorOffset = authoring.AreaEffectorOffset,
                Projectile = authoring.Projectile,
                PojectileRate = authoring.PojectileRate,
                PojectileOffset = authoring.PojectileOffset,
                RayCast = authoring.RayCast,
                RayCastRate = authoring.RayCastRate,
                RayCastOffset = authoring.RayCastOffset
            });
		}
	}
}