using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerInventoryManagerInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<InventoryManager>().FromNew().AsSingle().NonLazy();
        SystemBaseZenjectUtility.InjectSystemBases<ItemPickupSystemBase>(Container);
    }
}
