using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Zenject;

public class GameSettings
{
    public IPlayerConfig PlayerConfig;
    public GameSettings(IPlayerConfig playerConfig)
    {
        PlayerConfig = playerConfig;
    }

    private void InjectPlayerEntities(PlayerConfig playerConfig)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;
        var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlayerTag, MovementData>();
        var query = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(entityQuery);
        Debug.Log("entity count: " + query.CalculateEntityCount());
        var entities = query.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            var currentMovementData = entityManager.GetComponentData<MovementData>(entity);
            currentMovementData.MoveSpeed = playerConfig.MoveSpeed;
            currentMovementData.TurnSpeed = playerConfig.TurnSpeed;
            entityManager.SetComponentData(entity, currentMovementData);
            Debug.Log("Injected");
        }
    }
}
