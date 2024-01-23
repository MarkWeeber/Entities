using Unity.Entities;

public struct SystemControllerData : IComponentData
{
    public bool Player;
    public float PlayerRate;
    public bool Animation;
    public float AnimationRate;
    public bool AI;
    public float AIRate;
    public bool AreaEffector;
    public float AreaEffectorRate;
    public bool Projectile;
    public float PojectileRate;
    public bool RayCast;
    public float RayCastRate;
}