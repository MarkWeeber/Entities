using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class PlayerBodyPositioning : MonoBehaviour
{
    private Entity targetEntity;
    
    private void LateUpdate()
    {
        targetEntity = GetTargetEntity();
        if (targetEntity != Entity.Null)
        {
            Vector3 postion = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalToWorld>(targetEntity).Position;
            Quaternion rotation = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalToWorld>(targetEntity).Rotation;
            transform.position = postion;
            transform.rotation = rotation;
        }        
    }

    private Entity GetTargetEntity()
    {
        Entity responseEntity = Entity.Null;
        EntityQuery query = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(PlayerTag));
        NativeArray<Entity> entityArray = query.ToEntityArray(Allocator.Temp);
        if (entityArray.Length > 0)
        {
            responseEntity = entityArray[0];
        }
        return responseEntity;
    }
}
