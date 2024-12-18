using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputHandler", menuName = "Scriptable Objects/InputHandler")]
public class InputHandler: ScriptableObject, InputSystem_Actions.IPlayerActions
{
    private static InputSystem_Actions PlayerControls;

    public Vector2 MousePosition { get; private set; }
    public Action OnSelect { get; set; } = delegate { };

    private void OnEnable()
    {
        if (PlayerControls == null)
        {
            PlayerControls = new InputSystem_Actions();
            PlayerControls.Player.SetCallbacks(this);
            PlayerControls.Enable();
        }
    }

    private void OnDisable()
    {
        PlayerControls.Disable();
    }

    void InputSystem_Actions.IPlayerActions.OnMousePointer(InputAction.CallbackContext context)
    {
        MousePosition = context.ReadValue<Vector2>();
    }

    void InputSystem_Actions.IPlayerActions.OnSelect(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnSelect.Invoke();
        }
    }
}
