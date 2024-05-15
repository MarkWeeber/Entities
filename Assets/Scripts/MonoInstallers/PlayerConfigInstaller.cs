using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Zenject;

public class PlayerConfigInstaller : MonoInstaller
{
    [SerializeField]
    private PlayerConfig playerConfig;
    [SerializeField]
    private bool SwitchToDummySettings = false;
    public override void InstallBindings()
    {
        if (SwitchToDummySettings && playerConfig != null)
        {
            Container.Bind<IPlayerConfig>().To<PlayerConfigDummy>()
                .AsSingle().NonLazy(); // bind player config
        }
        else
        {
            Container.Bind<IPlayerConfig>().To<PlayerConfig>()
                .FromInstance(playerConfig).AsSingle().NonLazy(); // bind player config
        }
        Container.Bind<GameSettings>().AsSingle().NonLazy(); // instantiate game settings
        SystemBaseZenjectUtility.InjectSystemBases<GameSettingSystemBase>(Container);
    }
}
