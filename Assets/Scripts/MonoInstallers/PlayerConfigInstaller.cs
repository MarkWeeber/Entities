using Zenject;

public class PlayerConfigInstaller : MonoInstaller
{
    public PlayerConfig PlayerConfig;
    public override void InstallBindings()
    {
        Container.Bind<PlayerConfig>().FromInstance(PlayerConfig).AsSingle().NonLazy(); // create single player config
        Container.Bind<GameSettings>().AsSingle().NonLazy(); // create game settings right away
    }
}
