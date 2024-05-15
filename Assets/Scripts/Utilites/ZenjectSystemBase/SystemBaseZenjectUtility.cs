using Unity.Entities;
using Zenject;

public static class SystemBaseZenjectUtility
{
    public static void InjectSystemBases<T>(DiContainer diContainer) where T : SystemBase
    {
        var world = World.DefaultGameObjectInjectionWorld;
        diContainer.Inject(world.GetExistingSystemManaged<T>());
    }
}
