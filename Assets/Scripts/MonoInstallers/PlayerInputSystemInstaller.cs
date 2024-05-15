using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Zenject;

public class PlayerInputSystemInstaller : MonoInstaller
{
    private Controls controls;
    public override void InstallBindings()
    {
        controls = new Controls();
        controls.Enable();
        Container.Bind<Controls>().FromInstance(controls).AsSingle().NonLazy();
        SystemBaseZenjectUtility.InjectSystemBases<PlayerInputSystem>(Container);
    }

    private void OnDestroy()
    {
        controls.Disable();
    }
}

