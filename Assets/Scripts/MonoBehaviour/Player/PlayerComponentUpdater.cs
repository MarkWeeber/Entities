using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

public class PlayerComponentUpdater : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private TMP_Text coinsCollectedText;
    private Entity targetEntity;


    private void LateUpdate()
    {
        targetEntity = GetTargetEntity();
        if (targetEntity != Entity.Null)
        {
            UpdatePositioning();
            UpdateHealthBar();
            UpdateCoinsCollected();
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

    private void UpdatePositioning()
    {
        Vector3 postion = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalToWorld>(targetEntity).Position;
        Quaternion rotation = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalToWorld>(targetEntity).Rotation;
        transform.position = postion;
        transform.rotation = rotation;
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            float currentHealth = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<HealthData>(targetEntity).CurrentHealth;
            float maxHealth = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<HealthData>(targetEntity).MaxHealth;
            float healtBarRatio = Mathf.Clamp(0, currentHealth / maxHealth, 1f);
            healthBar.fillAmount = healtBarRatio;
        }
    }

    private void UpdateCoinsCollected()
    {
        if (coinsCollectedText != null)
        {
            uint currentCoinsCollected = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<CollectibleData>(targetEntity).CoinsCollected;
            coinsCollectedText.text = currentCoinsCollected.ToString();
        }
    }
}
