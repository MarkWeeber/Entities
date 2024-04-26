using Unity.Entities;

public struct ConstantSpinningData : IComponentData, IEnableableComponent
{
    public float YSpinAngle;
    public float HeightPhase;
    public float HeightPhaseSpeed;
}