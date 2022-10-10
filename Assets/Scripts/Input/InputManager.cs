using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Player;

[DefaultExecutionOrder(-100)]
public class InputManager : Singleton<InputManager>
{
  public delegate void JumpAction();
  public event JumpAction OnJump;

  public delegate void MoveAction(Vector2 swipeDelta);
  public event MoveAction OnMove;

  public delegate void WindAction(Vector2 swipeDelta);
  public event WindAction OnThrowWind;

  private TouchControls touchControls;

  private InputAction tapAction;
  private InputAction moveAction;
  private InputAction windAction;
  private InputAction jumpAction;

  private Vector2 lastWindSwipeDelta;

  public FrameInput FrameInput { get; private set; }

  private void Awake()
  {
    Debug.Log("InputManager Awake");
    EnhancedTouchSupport.Enable();
    touchControls = new TouchControls();
  }

  private void OnEnable()
  {
    touchControls.Enable();
    tapAction = touchControls.Touch.TouchTap;
    moveAction = touchControls.Touch.Move;
    windAction = touchControls.Touch.Wind;
    jumpAction = touchControls.Touch.Jump;
  }

  private void OnDisable()
  {
    touchControls.Disable();
    EnhancedTouchSupport.Disable();
  }

  private void Start()
  {
    tapAction.performed += ctx => PerformTap(ctx);
    moveAction.performed += ctx => PerformMove(ctx);
    windAction.performed += ctx => PerformWind(ctx);
    windAction.canceled += ctx => PerformWind(ctx);
  }

  private void PerformMove(InputAction.CallbackContext context)
  {
    Vector2 joystickDelta = context.ReadValue<Vector2>();
    OnMove?.Invoke(joystickDelta);
  }

  private void PerformTap(InputAction.CallbackContext context)
  {
    PointerInput pointerInput = context.ReadValue<PointerInput>();
    if (pointerInput.Contact)
    {
      if (!IsJoystickTouch(pointerInput.Position))
      {
        OnJump?.Invoke();
      }
    }
  }

  private void PerformWind(InputAction.CallbackContext context)
  {
    Vector2 windDelta = context.ReadValue<Vector2>();
    if (context.canceled)
    {
      OnThrowWind?.Invoke(lastWindSwipeDelta);
      lastWindSwipeDelta = Vector2.zero;
      return;
    }

    if (Mathf.Abs(windDelta.x) >= 0.5f || Mathf.Abs(windDelta.y) >= 0.5f)
    {
      lastWindSwipeDelta = windDelta;
    }
  }

  private void Update() => FrameInput = Gather();

  private FrameInput Gather()
  {
    return new FrameInput
    {
      JumpDown = jumpAction.WasPressedThisFrame(),
      Move = moveAction.ReadValue<Vector2>(),
      Wind = windAction.ReadValue<Vector2>(),
    };
  }

  private bool IsJoystickTouch(Vector2 position)
  {
    return (position.x < Screen.width / 2);
  }
}
