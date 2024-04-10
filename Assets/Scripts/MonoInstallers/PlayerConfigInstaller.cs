using System.Collections.Generic;
using Unity.Entities;
using Zenject;

public class PlayerConfigInstaller : MonoInstaller
{
    public PlayerConfig PlayerConfig;
    public override void InstallBindings()
    {
        Container.Bind<PlayerConfig>().FromInstance(PlayerConfig).AsSingle().NonLazy(); // bind player config
        Container.Bind<GameSettings>().AsSingle().NonLazy(); // instantiate game settings
        InjectSystemBases();
    }

    private void InjectSystemBases()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        Container.Inject(world.GetExistingSystemManaged<GameSettingSystemBase>());
    }
}
