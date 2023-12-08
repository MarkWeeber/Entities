using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class PlayerInputSystem : SystemBase
{
    private Controls controls;
    private bool firing;
    private bool sprinting;

    protected override void OnCreate()
    {
        RequireForUpdate<PlayerInputData>();
        controls = new Controls();
    }

    protected override void OnStartRunning()
    {
        controls.Enable();
        controls.Player.Fire.performed += FirePerformed;
        controls.Player.Fire.canceled += FireCancelled;
        controls.Player.Sprint.performed += SprintPerformed;
        controls.Player.Sprint.canceled += SprintCancelled;
    }

    protected override void OnDestroy()
    {
        controls.Player.Fire.performed -= FirePerformed;
        controls.Player.Fire.canceled -= FireCancelled;
        controls.Player.Sprint.performed -= SprintPerformed;
        controls.Player.Sprint.canceled -= SprintCancelled;
        controls.Disable();
    }
    
    protected override void OnUpdate()
    {
        RefRW<PlayerInputData> playerInputData = SystemAPI.GetSingletonRW<PlayerInputData>();
        playerInputData.ValueRW.MovementVector = controls.Player.Move.ReadValue<Vector2>();
        playerInputData.ValueRW.Firing = firing;
        playerInputData.ValueRW.Sprinting = sprinting;
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
}