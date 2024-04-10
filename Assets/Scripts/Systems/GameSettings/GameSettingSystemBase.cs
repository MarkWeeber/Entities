using Unity.Entities;
using UnityEngine;
using Zenject;

public partial class GameSettingSystemBase : SystemBase
{
    private float _moveSpeed;
    private float _turnSpeed;
    private bool injected;

    [Inject]
    private void Init(GameSettings gameSettings)
    {
        _moveSpeed = gameSettings.MoveSpeed;
        _turnSpeed = gameSettings.TurnSpeed;
        injected = true;
        
    }

    protected override void OnUpdate()
    {
        if (injected)
        {
            foreach ((PlayerTag playerTag, RefRW<MovementData> movementData) 
                in SystemAPI.Query<PlayerTag, RefRW<MovementData>>())
            {
                movementData.ValueRW.MoveSpeed = _moveSpeed;
                movementData.ValueRW.TurnSpeed = _turnSpeed;
            }
            Enabled = false;
            injected = false;
        }
    }
}