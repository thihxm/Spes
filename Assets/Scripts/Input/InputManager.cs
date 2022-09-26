using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

[DefaultExecutionOrder(-100)]
public class InputManager : Singleton<InputManager>
{
  public delegate void JumpAction();
  public event JumpAction OnJump;

  public delegate void MoveAction(Vector2 swipeDelta);
  public event MoveAction OnMove;

  public delegate void WindAction(Direction windDirection);
  public event WindAction OnThrowWind;
  
  private TouchControls touchControls;

  private InputAction tapAction;
  private InputAction moveAction;
  private InputAction windAction;

  private Vector2 lastWindSwipeDelta;

  private void Awake() {
    Debug.Log("InputManager Awake");
    EnhancedTouchSupport.Enable();
    touchControls = new TouchControls();
  }

  private void OnEnable() {
    touchControls.Enable();
    tapAction = touchControls.Touch.TouchTap;
    moveAction = touchControls.Touch.Move;
    windAction = touchControls.Touch.Wind;
  }

  private void OnDisable() {
    touchControls.Disable();
    EnhancedTouchSupport.Disable();
  }

  private void Start() {
    // touchControls.Touch.TouchSwipeDelta.performed += ctx => PerformSwipe(ctx);
    tapAction.performed += ctx => PerformTap(ctx);
    moveAction.performed += ctx => PerformMove(ctx);
    windAction.performed += ctx => PerformWind(ctx);
    windAction.canceled += ctx => PerformWind(ctx);
  }

  private void PerformMove(InputAction.CallbackContext context) {
    Vector2 joystickDelta = context.ReadValue<Vector2>();
    Vector2 xInput = new Vector2(joystickDelta.x, 0);
    OnMove?.Invoke(xInput);
  }

  private void PerformTap(InputAction.CallbackContext context) {
    PointerInput pointerInput = context.ReadValue<PointerInput>();
    if (pointerInput.Contact) {
      if (!IsJoystickTouch(pointerInput.Position)) {
        OnJump?.Invoke();
      }
    }
  }

  private void PerformWind(InputAction.CallbackContext context) {
    Vector2 windDelta = context.ReadValue<Vector2>();
    if (context.canceled) {
      Direction actionDirection = GetDirection(lastWindSwipeDelta);
      OnThrowWind?.Invoke(actionDirection);
      lastWindSwipeDelta = Vector2.zero;
      return;
    }

    if (Mathf.Abs(windDelta.x) >= 0.5f || Mathf.Abs(windDelta.y) >= 0.5f) {
      lastWindSwipeDelta = windDelta;
    }
  }

  private Direction GetDirection(Vector2 windDelta) {
    float x = windDelta.x;
    float y = windDelta.y;
    Direction actionDirection;

    Debug.Log("windDelta: " + windDelta);

    if (x == 0 && y == 0) {
      return Direction.Stationary;
    }
    
    if (Mathf.Abs(y) > Mathf.Abs(x)) {
      if (y > 0) {
        actionDirection = Direction.Up;
      } else {
        actionDirection = Direction.Down;
      }
    } else {
      if (x > 0) {
        actionDirection = Direction.Right;
      } else {
        actionDirection = Direction.Left;
      }
    }

    return actionDirection;
  }

  private bool IsJoystickTouch(Vector2 position) {
    return (position.x < Screen.width / 2);
  }

  private void Update() {
    
  }
}
