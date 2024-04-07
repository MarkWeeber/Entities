using Zenject;

public class PlayerConfigInstaller : MonoInstaller
{
    public PlayerConfig PlayerConfig;
    public override void InstallBindings()
    {
        Container.Bind<PlayerAuthoring>().AsSingle();
    }
}
