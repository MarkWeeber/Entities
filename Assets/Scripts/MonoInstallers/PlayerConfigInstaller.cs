using System.Collections.Generic;
using Unity.Entities;
using Zenject;

public class PlayerConfigInstaller : MonoInstaller
{
    public PlayerConfig PlayerConfig;
    public override void InstallBindings()
    {
        Container.Bind<PlayerConfig>().FromInstance(PlayerConfig).AsSingle().NonLazy(); // create single player config
        InjectSystemBases();

    }

    

    private void InjectSystemBases()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        Container.Inject(world.GetExistingSystemManaged<GameSettingSystemBase>());
        //Container.Inject(world.GetExistingSystem<GameSettingSystemBase>());
        //Container.Inject(world.GetOrCreateSystem<GameSettingSystemBase>());
        //systemBases.Add(world.GetExistingSystemManaged<GameSettingSystemBase>());
        //foreach (var item in systemBases)
        //{
        //    Container.Inject(item);
        //}
    }
}
