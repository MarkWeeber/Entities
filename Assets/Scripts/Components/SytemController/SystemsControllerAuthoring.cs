using Unity.Entities;
using UnityEngine;

public class SystemsControllerAuthoring : MonoBehaviour
{
    public bool Player = true;
    public float PlayerRate = 0f;
    public bool AI = true;
    public float AIRate = 0f;
    public bool AreaEffector = true;
    public float AreaEffectorRate = 0f;
    public bool Projectile = true;
    public float PojectileRate = 0f;
    public bool RayCast = true;
    public float RayCastRate = 0f;

    class Baker : Baker<SystemsControllerAuthoring>
	{
		public override void Bake(SystemsControllerAuthoring authoring)
		{
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new SystemControllerData
            {
                Player = authoring.Player,
                PlayerRate = authoring.PlayerRate,
                AI = authoring.AI,
                AIRate = authoring.AIRate,
                AreaEffector = authoring.AreaEffector,
                AreaEffectorRate = authoring.AreaEffectorRate,
                Projectile = authoring.Projectile,
                PojectileRate = authoring.PojectileRate,
                RayCast = authoring.RayCast,
                RayCastRate = authoring.RayCastRate
            });
		}
	}
}