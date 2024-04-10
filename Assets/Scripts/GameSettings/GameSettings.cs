using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Zenject;

public class GameSettings
{
    public float MoveSpeed = 1.0f;
    public float TurnSpeed = 5.0f;
    public GameSettings(PlayerConfig playerConfig)
    {
        if (playerConfig != null)
        {
            MoveSpeed = playerConfig.MoveSpeed;
            TurnSpeed = playerConfig.TurnSpeed;
        }
        else
        {
            MoveSpeed = 2.0f;
            TurnSpeed = 10f;
        }
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
