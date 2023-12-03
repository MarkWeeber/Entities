//using System;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using static Controls;

//[CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/Input Reader")]
//public class InputReader : ScriptableObject, IPlayerActions
//{
//    public Action<bool> PlayerFireEvent;
//    public Action<Vector2> PlayerMovementEvent;
//    public Action<bool> PlayerDashEvent;

//    private Controls controls;

//    private void OnEnable()
//    {
//        if (controls == null)
//        {
//            controls = new Controls();
//            controls.Player.SetCallbacks(this);
//        }
//        controls.Player.Enable();
//    }

//    public void OnDash(InputAction.CallbackContext context)
//    {
//        if (context.performed)
//        {
//            PlayerDashEvent?.Invoke(true);
//        }
//        else if (context.canceled)
//        {
//            PlayerDashEvent?.Invoke(false);
//        }
//    }

//    public void OnFire(InputAction.CallbackContext context)
//    {
//        if (context.performed)
//        {
//            PlayerFireEvent?.Invoke(true);
//        }
//        else if (context.canceled)
//        {
//            PlayerFireEvent?.Invoke(false);
//        }
//    }

//    public void OnMove(InputAction.CallbackContext context)
//    {
//        PlayerMovementEvent?.Invoke(context.ReadValue<Vector2>());
//    }
//}
