using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class PlayerInputSystem : SystemBase
{
    private Controls controls;
    private bool firing;
    private bool sprinting;
    private bool fireRelease;
    private bool sprintRelease;

    protected override void OnCreate()
    {
        RequireForUpdate<PlayerInputData>();
        controls = new Controls();
    }

    protected override void OnStartRunning()
    {
        controls.Player.Fire.performed += FirePerformed;
        controls.Player.Fire.canceled += FireCancelled;
        controls.Player.Dash.performed += DashPerformed;
        controls.Player.Dash.canceled += DashCancelled;
        controls.Enable();
    }

    protected override void OnDestroy()
    {
        controls.Player.Fire.performed -= FirePerformed;
        controls.Player.Fire.canceled -= FireCancelled;
        controls.Player.Dash.performed -= DashPerformed;
        controls.Player.Dash.canceled -= DashCancelled;
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
        if (fireRelease)
        {
            firing = true;
        }
        else
        {
            firing = false;
        }
    }

    private void FireCancelled(InputAction.CallbackContext context)
    {
        firing = false;
        fireRelease = true;
    }

    private void DashPerformed(InputAction.CallbackContext context)
    {
        if (sprintRelease)
        {
            sprinting = true;
        }
        else
        {
            sprinting = false;
        }
    }

    private void DashCancelled(InputAction.CallbackContext context)
    {
        sprinting = false;
        sprintRelease = true;
    }
}