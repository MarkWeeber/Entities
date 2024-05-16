using Unity.Entities;

public interface IConsumable
{
    void Consume(Entity entity, EntityManager entityManager);
}

