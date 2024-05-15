using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class PlayerInputSystem : SystemBase
{
    private Controls controls;
    private bool firing;
    private bool sprinting;
    private bool injectSuccess;

    [Inject]
    private void Init(Controls controls)
    {
        this.controls = controls;
        injectSuccess = true;
        SubscribeActions();
    }

    protected override void OnCreate()
    {
        RequireForUpdate<PlayerInputData>();
    }

    protected override void OnStartRunning()
    {
    }

    protected override void OnDestroy()
    {
        if (injectSuccess)
        {
            controls.Player.Fire.performed -= FirePerformed;
            controls.Player.Fire.canceled -= FireCancelled;
            controls.Player.Sprint.performed -= SprintPerformed;
            controls.Player.Sprint.canceled -= SprintCancelled;
        }
    }
    
    protected override void OnUpdate()
    {
        if (injectSuccess)
        {
            RefRW<PlayerInputData> playerInputData = SystemAPI.GetSingletonRW<PlayerInputData>();
            playerInputData.ValueRW.MovementVector = controls.Player.Move.ReadValue<Vector2>();
            playerInputData.ValueRW.Firing = firing;
            playerInputData.ValueRW.Sprinting = sprinting;
        }
    }

    private void FirePerformed(InputAction.CallbackContext context)
    {
        firing = true;
    }

    private void FireCancelled(InputAction.CallbackContext context)
    {
        firing = false;
    }

    private void SprintPerformed(InputAction.CallbackContext context)
    {
        sprinting = true;
    }

    private void SprintCancelled(InputAction.CallbackContext context)
    {
        sprinting = false;
    }

    private void SubscribeActions()
    {
        controls.Player.Fire.performed += FirePerformed;
        controls.Player.Fire.canceled += FireCancelled;
        controls.Player.Sprint.performed += SprintPerformed;
        controls.Player.Sprint.canceled += SprintCancelled;
    }
}