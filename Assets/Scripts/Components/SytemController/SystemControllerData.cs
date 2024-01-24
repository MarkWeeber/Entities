using Unity.Entities;

public struct SystemControllerData : IComponentData
{
    public bool Player;
    public float PlayerRate;
    public float PlayerOffset;
    public bool Animation;
    public float AnimationRate;
    public float AnimationOffset;
    public bool AI;
    public float AIRate;
    public float AIOffset;
    public bool AreaEffector;
    public float AreaEffectorRate;
    public float AreaEffectorOffset;
    public bool Projectile;
    public float PojectileRate;
    public float PojectileOffset;
    public bool RayCast;
    public float RayCastRate;
    public float RayCastOffset;
}