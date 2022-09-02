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

  public delegate void MoveAction(Vector2 direction);
  public event MoveAction OnMove;
  
  private TouchControls touchControls;

  private void Awake() {
    Debug.Log("InputManager Awake");
    EnhancedTouchSupport.Enable();
    touchControls = new TouchControls();
  }

  private void OnEnable() {
    touchControls.Enable();
  }

  private void OnDisable() {
    touchControls.Disable();
    EnhancedTouchSupport.Disable();
  }

  private void Start() {
    touchControls.Touch.TouchSwipe.performed += ctx => PerformSwipe(ctx);
    touchControls.Touch.TouchTap.performed += ctx => PerformTap(ctx);
  }

  private void PerformTap(InputAction.CallbackContext context) {
    PointerInput pointerInput = context.ReadValue<PointerInput>();
    if (pointerInput.Contact) {
      if (!IsJoystickTouch(pointerInput.Position)) {
        if (OnJump != null) {
          OnJump();
        }
      }
    }
  }

  private void PerformSwipe(InputAction.CallbackContext context) {
    // Debug.Log("Swipe delta: " + touchControls.Touch.TouchSwipe.type + " " + touchControls.Touch.TouchSwipe.ReadValue<Vector2>());
    // if (IsJoystickTouch()) {
    //   if (OnMove != null) {
    //     OnMove(context.ReadValue<Vector2>());
    //   }
    // }
  }

  private bool IsJoystickTouch(Vector2 position) {
    return (position.x < Screen.width / 2);
  }

  private void Update() {
    
  }
}
